import csv
import re

# small_lex.csv のパス
INPUT_FILES = [
    "small_lex.csv",
    "core_lex.csv",
    "notcore_lex.csv"
]
OUTPUT_CSV    = "best_shiritori_words.csv"

# カタカナ → ひらがな
KATAKANA_START = ord("ァ")
KATAKANA_END   = ord("ヶ")
KATAKANA_TO_HIRA_DIFF = ord("ぁ") - ord("ァ")

def kata_to_hira(s: str) -> str:
    result = []
    for ch in s:
        code = ord(ch)
        if KATAKANA_START <= code <= KATAKANA_END:
            result.append(chr(code + KATAKANA_TO_HIRA_DIFF))
        else:
            result.append(ch)
    return "".join(result)

# ひらがなだけかどうか
HIRAGANA_RE = re.compile(r"^[ぁ-ゖー]+$")

def is_all_hiragana(s: str) -> bool:
    return bool(HIRAGANA_RE.match(s))

# 記号やアルファベットが含まれるものは除く

SURFACE_RE = re.compile(r'^[\u3040-\u309F\u30A0-\u30FF\u4E00-\u9FFFー]+$')

def is_valid_surface(surface: str) -> bool:
    return bool(SURFACE_RE.match(surface))

# 優先度のためのスコア付け

def score_word(pos2, compound) -> int:
    if pos2 == "普通名詞":
        pos_score = 10
    else:
        pos_score = 0

    if compound == "A":
        com_score = 3
    elif compound == "B":
        com_score = 2
    else:
        com_score = 1

    return pos_score + com_score


def main():
    seen = {}  # 重複除去用（reading）

    for input_path in INPUT_FILES:
        print(f"Loading: {input_path}")

        with open(input_path, encoding="utf-8") as f_in:

            reader = csv.reader(f_in)
            
            for row in reader:
                if not row:
                    continue

                surface = row[0]          # 表記
                pos1    = row[5]          # 名詞/動詞
                pos2    = row[6]          # 固有名詞/普通名詞/数詞
                pos3    = row[7]          # 一般/地名/人名...
                reading = row[11]         # カタカナ読み
                compound = row[14]        # 複合語度判定(A/B/C)

                # --- ここからフィルタルール（好みで調整） ---

                # 名詞だけに絞る（しりとり用）
                if pos1 != "名詞":
                    continue

                # 数詞はなし
                if pos2 == "数詞":
                    continue

                # 人名はなし
                if pos3 == "人名":
                    continue

                # 複合語レベルの高い地名はなし
                if pos3 == "地名" and compound != "A":
                    continue

                # 読みをひらがなに統一
                reading_hira = kata_to_hira(reading)

                # 読みがひらがな以外を含む単語を除外（ハングルなど）
                if not is_all_hiragana(reading_hira):
                    continue

                # 表記にアルファベットや記号が含まれる単語を除外
                if not is_valid_surface(surface):
                    continue
                
                # 「ん」で終わるものを除外
                if reading_hira.endswith("ん"):
                    continue

                key = reading_hira

                score = score_word(pos2, compound)

                if key not in seen:
                    seen[key] = (surface, pos2, pos3, compound, score)
                else:
                    # 既存のものより優先度が高ければ置き換え
                    old_surface,_,_,_,old_score = seen[key]
                    if score > old_score:
                        seen[key] = (surface, pos2, pos3, compound, score)
                    
                    # 既存のものと優先度が同じであれば追加
                    elif score == old_score:
                        surfaces = old_surface.split("/")
                        if surface not in surfaces:
                            new_surface = old_surface + "/" + surface
                            seen[key] = (new_surface, pos2, pos3, compound, score)

    with open(OUTPUT_CSV, "w", encoding="utf-8", newline="") as f_out:

        writer = csv.writer(f_out)

        # ヘッダ
        writer.writerow(["surface", "reading", "pos1", "pos2", "pos3", "compound"])

        for reading, (surface, pos2, pos3, compound, score) in seen.items():
            writer.writerow([reading, surface, pos2, pos3, compound])


    print("done:", OUTPUT_CSV)

if __name__ == "__main__":
    main()
