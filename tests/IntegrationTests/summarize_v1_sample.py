"""Summarize the isolated AutoCAD sample runs without changing survey data."""
import argparse
import collections
import csv
import hashlib
import json
import re
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--qa', type=Path, required=True)
parser.add_argument('--source', type=Path, required=True)
args = parser.parse_args()
qa = args.qa

def read(path):
    return path.read_text(encoding='utf-8-sig')

def records(path):
    out = {}
    for line in read(path).splitlines():
        match = re.match(r'^\("([^"\n]+)"', line)
        if not match:
            continue
        pairs = re.findall(r'\("([^"\n]+)" \. "((?:\\.|[^"\\])*)"\)', line)
        out[match[1]] = {key: value.replace('\\"', '"').replace('\\\\', '\\') for key, value in pairs}
    return out

def comparable(path):
    return re.sub(r'\("sua_luc" \. "[^"]*"\)', '', read(path))

before = qa / 'source-reference'
after = qa / 'reopened-261011'
assert comparable(before / 'field-after.txt') == comparable(after / 'field-after.txt'), 'Legacy calculation differs'
assert comparable(qa / 'final-261011/field-after.txt') == comparable(after / 'field-before.txt'), 'Save/reopen differs'
assert read(before / 'points-before.txt') == read(after / 'points-before.txt') == read(after / 'points-after.txt'), 'Point coordinates/source differ'
source_hash = hashlib.sha256(args.source.read_bytes()).hexdigest().upper()
assert source_hash == '6DBAC2D05968FF69CF23AC483A68EE759AD749E2786CE570519F9A3AF52367EB', 'Original sample changed'

old = records(before / 'field-before.txt')
new = records(after / 'field-after.txt')
assert len(new) == 177
groups = collections.Counter(r.get('nhom', '') for r in new.values())
reasons = collections.Counter()
rows = []
for key, record in new.items():
    error = record.get('field_last_error', '')
    if error.startswith('Mốc '):
        category = 'Khoảng cách hoặc hệ số giữa cọc cần đối chiếu'
    elif error.startswith('Không có đoạn cọc'):
        category = 'Chưa có cặp cọc bao quanh vị trí'
    elif error.startswith('Đoạn có cọc trùng'):
        category = 'Trùng lý trình hoặc thứ tự vị trí bất thường'
    elif error.startswith('Điểm thuộc phạm vi nhiều'):
        category = 'Có nhiều đoạn cọc cùng phù hợp'
    elif error:
        category = 'Chưa có nhóm hoặc cặp mốc hợp lệ'
    else:
        category = ''
    if category:
        reasons[category] += 1
    rows.append([key, record.get('nhom', ''), record.get('ma_hieu', ''),
                 old[key].get('ly_trinh_km', ''), record.get('ly_trinh_km', ''),
                 record.get('station_status', ''), record.get('field_used_a', ''),
                 record.get('field_used_b', ''), record.get('field_manual_control', ''), category, error])
out = qa / 'delivery'
out.mkdir(exist_ok=True)
with (out / 'HO_SO_CAN_KIEM_TRA.csv').open('w', encoding='utf-8-sig', newline='') as stream:
    writer = csv.writer(stream)
    writer.writerow(['ID hồ sơ', 'Nhóm', 'Mã biển', 'Lý trình trước thử', 'Lý trình sau thử',
                     'Trạng thái', 'Mốc đầu', 'Mốc cuối', 'Có mốc thủ công', 'Nhóm việc cần làm', 'Chi tiết'])
    writer.writerows(row for row in rows if row[-1])
with (out / 'TAT_CA_HO_SO.csv').open('w', encoding='utf-8-sig', newline='') as stream:
    writer = csv.writer(stream)
    writer.writerow(['ID hồ sơ', 'Nhóm', 'Mã biển', 'Lý trình trước thử', 'Lý trình sau thử',
                     'Trạng thái', 'Mốc đầu', 'Mốc cuối', 'Có mốc thủ công', 'Nhóm việc cần làm', 'Chi tiết'])
    writer.writerows(rows)
summary = dict(source_sha256=source_hash, objects=len(new), groups=dict(groups), reasons=dict(reasons),
               missing_station=sum(not r.get('ly_trinh_m') for r in new.values()),
               identical_to_legacy_except_timestamp=True, save_reopen_identical=True,
               coordinates_and_sources_unchanged=True)
(out / 'verification.json').write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(summary, ensure_ascii=False, indent=2))
