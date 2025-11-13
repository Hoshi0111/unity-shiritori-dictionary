using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace Shiritori.Dictionary
{
    public class ShiritoriDictionary : MonoBehaviour
    {
        // Resources 内の CSV のパス
        private const string CsvResourcePath = "Datasets/best_shiritori_words";

        // Singleton 的にアクセスしたい場合用
        public static ShiritoriDictionary Instance { get; private set; }

        // key: reading（ひらがな）
        private readonly Dictionary<string, ShiritoriEntry> dict = new Dictionary<string, ShiritoriEntry>();

        private void Awake()
        {
            // Singleton セット
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            LoadCsvFromResources();
        }

        /// 開始直後に実行
        /// Resources から CSV(TextAsset) を自動ロードして辞書を構築
        private void LoadCsvFromResources()
        {
            var csvFile = Resources.Load<TextAsset>(CsvResourcePath);
            if (csvFile == null)
            {
                Debug.LogError($"ShiritoriDictionary: Resources から CSV が見つかりませんでした: {CsvResourcePath}");
                return;
            }

            dict.Clear();

            var text = csvFile.text;
            var lines = text.Split('\n');

            bool isFirst = true;
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                // 1行目はヘッダのためスルー
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }

                var cols = line.Split(',');
                if (cols.Length < 5)
                {
                    Debug.LogWarning("列数が足りない行をスキップ: " + line);
                    continue;
                }

                var reading = cols[0].Trim();
                var surface = cols[1].Trim();
                var pos2 = cols[2].Trim();
                var pos3 = cols[3].Trim();
                var compound = cols[4].Trim();

                if (string.IsNullOrEmpty(reading))
                    continue;

                var entry = new ShiritoriEntry
                {
                    Reading = reading,
                    Surface = surface,
                    Pos2 = pos2,
                    Pos3 = pos3,
                    Compound = compound
                };

                dict[reading] = entry;
            }

            Debug.Log($"ShiritoriDictionary: CSV 読み込み完了。エントリ数 = {dict.Count}");
        }

        // ======================
        //    公開 API 群
        // ======================

        /// <summary>
        /// 指定した読みが辞書に存在するかどうか
        /// </summary>
        public bool HasReading(string reading)
        {
            if (string.IsNullOrEmpty(reading)) return false;
            return dict.ContainsKey(reading);
        }

        /// <summary>
        /// 読みが一致するエントリの surface をそのまま返す
        /// </summary>
        public bool TryGetSurface(string reading, out string surface)
        {
            surface = null;
            if (string.IsNullOrEmpty(reading)) return false;

            if (dict.TryGetValue(reading, out var entry))
            {
                surface = entry.Surface;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 読みが一致するエントリの surface を "/" で分割して配列で返す
        /// </summary>
        public bool TryGetSurfaces(string reading, out string[] surfaces)
        {
            surfaces = null;
            if (string.IsNullOrEmpty(reading)) return false;

            if (dict.TryGetValue(reading, out var entry))
            {
                surfaces = entry.Surface.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 読みが一致するエントリの surfaceを返り値とする、ない場合はNullで返す
        /// </summary>
        public string GetSurfaceOrNull(string reading)
        {
            if (dict.TryGetValue(reading, out var entry))
            {
                return entry.Surface;   // 見つかった場合
            }
            return null;               // 見つからなかった場合
        }

        /// <summary>
        /// 読みが一致するエントリの surface を "/" で分割して配列で返り値とする、ない場合はNullで返す
        /// </summary>
        public string[] GetSurfaceListOrNull(string reading, out string[] surfaces)
        {
            surfaces = null;
            if (dict.TryGetValue(reading, out var entry))
            {
                surfaces = entry.Surface.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                return surfaces;
            }
            return null;
        }

        /// <summary>
        /// 条件付きで「読みが一致する有効な単語があるか」を確認
        /// allowProperNoun: false → 固有名詞を除外
        /// compoundFilter : All / Limited / Strict
        /// </summary>
        public bool HasReadingWithCondition(
            string reading,
            bool allowProperNoun = true,
            CompoundFilterMode compoundFilter = CompoundFilterMode.All)
        {
            if (string.IsNullOrEmpty(reading)) return false;

            if (!dict.TryGetValue(reading, out var entry))
                return false;

            if (!allowProperNoun && entry.Pos2 == "固有名詞")
                return false;

            if (!IsCompoundAllowed(entry.Compound, compoundFilter))
                return false;

            return true;
        }

        /// <summary>
        /// 条件付きで surface を取得する
        /// </summary>
        public bool TryGetSurfaceWithCondition(
            string reading,
            out string surface,
            bool allowProperNoun = true,
            CompoundFilterMode compoundFilter = CompoundFilterMode.All)
        {
            surface = null;
            if (string.IsNullOrEmpty(reading)) return false;

            if (!dict.TryGetValue(reading, out var entry))
                return false;

            if (!allowProperNoun && entry.Pos2 == "固有名詞")
                return false;

            if (!IsCompoundAllowed(entry.Compound, compoundFilter))
                return false;

            surface = entry.Surface;
            return true;
        }

        /// <summary>
        /// 条件付きで surface を取得し配列で返す
        /// </summary>
        public bool TryGetSurfaceListWithCondition(
            string reading,
            out string[] surfaces,
            bool allowProperNoun = true,
            CompoundFilterMode compoundFilter = CompoundFilterMode.All)
        {
            surfaces = null;
            if (string.IsNullOrEmpty(reading)) return false;

            if (!dict.TryGetValue(reading, out var entry))
                return false;

            if (!allowProperNoun && entry.Pos2 == "固有名詞")
                return false;

            if (!IsCompoundAllowed(entry.Compound, compoundFilter))
                return false;

            surfaces = entry.Surface.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return true;
        }

        // ======================
        //    優先度/条件ヘルパ
        // ======================

        private bool IsCompoundAllowed(string compound, CompoundFilterMode mode)
        {
            switch (mode)
            {
                case CompoundFilterMode.Strict:
                    return compound == "A";

                case CompoundFilterMode.Limited:
                    return compound == "A" || compound == "B";

                case CompoundFilterMode.All:
                    return compound == "A" || compound == "B" || compound == "C";

                default:
                    return true;
            }
        }
    }
}
