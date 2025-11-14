# Unity Shiritori Dictionary

A Japanese dictionary module optimized for shiritori games in Unity. 
しりとりの単語を判定するUnity用のモジュールです。

This package includes:
- A high-quality dictionary generated from **SudachiDict**
- Fast lookup API (C#)
- Hiragana-based word matching
- Filters for:
  - Proper noun (ON/OFF)
  - Compound noun (All/Limited/Strict)
- Unity-ready module + Demo scene
- Python script to regenerate the dictionary from SudachiDict

---

## Features

- ひらがなからしりとりに使用できる単語が利用できるか判定します
  - "ん"で終わる単語は除外
  - 人名は除外
  - 記号やアルファベットが含まれる単語は除外
  - 数詞は除外(1,2,3など)
- 単語が存在する場合、単語の見た目を返します（はんてい　→　判定/半弟/反帝/藩邸）
- 指定したひらがなから始まる単語をランダムで一つ選択して返します
  - フィルタリング条件付きで選択することも可能
  - 条件を満たす単語リストを返すことも可能
- フィルタリング機能
  - 固有名詞を許可／禁止  
  - 複合度レベル（三段階） 
- Unity統合
  - デモシーン 

## Installation

1. Clone or download this repository.
2. Copy the following folders into your Unity project:
   	Runtime/
	Resources/
	Demo/ (optional)
3. Open the demo scene or call `ShiritoriDictionary.Instance` from your code.

---

## Usage Example

### Basic Lookup

```csharp
using Shiritori.Dictionary;

void Example()
{
    var dict = ShiritoriDictionary.Instance;

    string reading = "しりとり";

    if (dict.HasReading(reading))
    {
        if (dict.TryGetSurface(reading, out string surface))
        {
            Debug.Log($"表記: {surface}");
        }
    }
}
```

### Filtered Lookup

```csharp
using Shiritori.Dictionary;

void Example()
{
    var dict = ShiritoriDictionary.Instance;

    // 固有名詞OKならtrue/固有名詞NGならfalse
    bool allowProperNoun = false;
    // 複合語を許容するならAll/複合語をある程度制限するならLimited/複合語を禁止するならStrict
    CompoundFilterMode filter = CompoundFilterMode.All;

	string reading = "しりとり";

    if (dict.HasReadingWithCondition(reading, allowProperNoun, filter))
    {
        dict.TryGetSurfaceWithCondition(reading, out string surface, allowProperNoun, filter);
        Debug.Log(surface);
    }
}
```

---

## License

### Code (C#, Python)
This repository's source code is licensed under the MIT License.

### Dictionary Data
The dictionary data (`shiritori_select_words.csv`) is generated from SudachiDict
and is distributed under the Apache License 2.0.

- SudachiDict: https://github.com/WorksApplications/SudachiDict
