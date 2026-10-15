using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace BHT.Core
{
    // Keep native CAD geometry intact, with a versioned and verified project envelope.
    public sealed class ProjectPackage : IDisposable
    {
        const long MaxDrawingBytes = 512L * 1024 * 1024;
        string directory;
        public string DrawingPath { get; private set; }

        static string Hash(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        public static void Write(string path, string drawingPath)
        {
            if (File.Exists(path)) throw new IOException("Tệp đã tồn tại. Chọn tên khác để giữ bản đã xuất.");
            long length = new FileInfo(drawingPath).Length;
            if (length == 0 || length > MaxDrawingBytes) throw new InvalidDataException("Dung lượng dự án BHT không hợp lệ (tối đa 512 MiB trước nén).");
            string full = Path.GetFullPath(path), temp = full + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write))
                using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
                {
                    var manifest = new XElement("BHTProject", new XAttribute("format", "1"),
                        new XAttribute("application", typeof(ProjectPackage).Assembly.GetName().Version.ToString()),
                        new XElement("Drawing", new XAttribute("file", "project.dwg"),
                            new XAttribute("bytes", length), new XAttribute("sha256", Hash(drawingPath))));
                    using (var output = zip.CreateEntry("manifest.xml").Open()) manifest.Save(output);
                    using (var input = File.OpenRead(drawingPath))
                    using (var output = zip.CreateEntry("project.dwg", CompressionLevel.Optimal).Open()) input.CopyTo(output);
                }
                // Reopen and verify before making the completed file visible at its final path.
                using (Open(temp)) { }
                File.Move(temp, full);
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }

        public static ProjectPackage Open(string path)
        {
            var package = new ProjectPackage();
            try
            {
                using (var file = File.OpenRead(path))
                using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                {
                    if (zip.Entries.Count != 2 || zip.Entries.Count(e => e.FullName == "manifest.xml") != 1 ||
                        zip.Entries.Count(e => e.FullName == "project.dwg") != 1)
                        throw new InvalidDataException("Không đúng cấu trúc dự án .bht hoặc tệp bị thiếu thành phần.");
                    var entry = zip.GetEntry("manifest.xml");
                    if (entry.Length > 16384) throw new InvalidDataException("Thông tin dự án BHT quá lớn.");
                    XElement manifest;
                    using (var input = entry.Open())
                    using (var reader = XmlReader.Create(input, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 16384 }))
                        manifest = XElement.Load(reader);
                    if (manifest.Name != "BHTProject" || (string)manifest.Attribute("format") != "1")
                        throw new InvalidDataException("Phiên bản định dạng .bht chưa được hỗ trợ. Cập nhật BHT hoặc xuất lại dự án.");
                    var drawing = manifest.Element("Drawing");
                    long expected;
                    if (drawing == null || (string)drawing.Attribute("file") != "project.dwg" ||
                        !long.TryParse((string)drawing.Attribute("bytes"), out expected) || expected <= 0 || expected > MaxDrawingBytes)
                        throw new InvalidDataException("Thông tin hình học trong dự án BHT không hợp lệ.");
                    entry = zip.GetEntry("project.dwg");
                    if (entry.Length != expected) throw new InvalidDataException("Dự án BHT bị thiếu hoặc sai dung lượng dữ liệu.");
                    package.directory = Path.Combine(Path.GetTempPath(), "BHT-project-" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(package.directory);
                    package.DrawingPath = Path.Combine(package.directory, "project.dwg");
                    using (var input = entry.Open())
                    using (var output = new FileStream(package.DrawingPath, FileMode.CreateNew, FileAccess.Write))
                    {
                        var buffer = new byte[81920]; long total = 0; int read;
                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            total += read;
                            if (total > expected) throw new InvalidDataException("Dữ liệu BHT vượt quá dung lượng khai báo.");
                            output.Write(buffer, 0, read);
                        }
                        if (total != expected) throw new InvalidDataException("Dữ liệu BHT bị cắt ngắn.");
                    }
                    if (!string.Equals(Hash(package.DrawingPath), (string)drawing.Attribute("sha256"), StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Dự án BHT bị hỏng hoặc đã bị thay đổi (SHA256 không khớp). Xuất lại từ bản nguồn.");
                }
                return package;
            }
            catch { package.Dispose(); throw; }
        }

        public void Dispose()
        {
            if (directory != null && Directory.Exists(directory)) Directory.Delete(directory, true);
            directory = null;
        }
    }
}
