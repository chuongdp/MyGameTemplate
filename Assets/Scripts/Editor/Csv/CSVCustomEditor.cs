using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Game.Script.Editor.Csv;
using Newtonsoft.Json;

public static class CsvHelper
{
    public static string EscapeCSV(string s)
    {
        if (string.IsNullOrEmpty(s))
            return "";

        if (!s.Contains(",") && !s.Contains("\"") && !s.Contains("\n") && !s.Contains("\r")) return s;

        s = s.Replace("\"", "\"\"");

        return $"\"{s}\"";
    }
}

[CustomEditor(typeof(TextAsset))]
public class CsvCustomEditor : Editor
{
    private const string Baseurl = "https://hungtrinh.app.n8n.cloud/webhook/5d275f66-3c31-429e-baa6-179986be4ef6?sheetUrl";

    private CsvDataConfig config;
    private string        currentKey;

    private void OnEnable()
    {
        this.config = CsvDataConfig.instance;
        var csvAsset = this.target as TextAsset;
        this.currentKey = AssetDatabase.GetAssetPath(csvAsset);
    }

    private Vector2 scrollTextAreaContext;

    public override void OnInspectorGUI()
    {
        var csvAsset  = this.target as TextAsset;
        var assetPath = AssetDatabase.GetAssetPath(csvAsset);

        if (!assetPath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            base.OnInspectorGUI();

            return;
        }

        GUI.enabled = true;

        this.config.data ??= new List<CsvDataConfig.CsvConfigData>();

        CsvDataConfig.CsvConfigData csvConfig = null;

        foreach (var csvConfigData in this.config.data.Where(csvConfigData => csvConfigData.NameCsv == this.target.name))
            csvConfig = csvConfigData;

        if (csvConfig == null)
        {
            csvConfig         = new CsvDataConfig.CsvConfigData();
            csvConfig.NameCsv = this.target.name;
            this.config.data.Add(csvConfig);
        }

        csvConfig.SheetUrl  = EditorGUILayout.TextField("Sheet URL", csvConfig.SheetUrl);
        csvConfig.SheetName = EditorGUILayout.TextField("Sheet Name", csvConfig.SheetName);

        if (GUILayout.Button("Sync Data from API"))
        {
            this.SyncData(assetPath, csvConfig.SheetUrl, csvConfig.SheetName);
            this.config.SaveSO();
        }

        GUILayout.Label("CSV Content:", EditorStyles.boldLabel);
        this.scrollTextAreaContext = EditorGUILayout.BeginScrollView(this.scrollTextAreaContext, GUILayout.Height(200));
        EditorGUILayout.TextArea(csvAsset.text, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        if (GUI.changed)
            EditorUtility.SetDirty(this.config);
    }

    /// <summary>
    /// Gọi API với thông tin cấu hình (Sheet URL và Sheet Name), parse JSON trả về, 
    /// chuyển thành CSV và ghi đè file CSV.
    /// </summary>
    private void SyncData(string assetPath, string sheetUrl, string sheetName)
    {
        // https://hungtrinh.app.n8n.cloud/webhook/5d275f66-3c31-429e-baa6-179986be4ef6?sheetUrl={sheetUrl}&sheetName={sheetName}
        var encodedSheetUrl  = UnityEngine.Networking.UnityWebRequest.EscapeURL(sheetUrl);
        var encodedSheetName = UnityEngine.Networking.UnityWebRequest.EscapeURL(sheetName);

        var apiUrl = $"{Baseurl}={encodedSheetUrl}&sheetName={encodedSheetName}";

        Debug.Log("Fetching from API: " + apiUrl);

        try
        {
            using var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            var json = client.DownloadString(apiUrl);
            Debug.Log("Received JSON: " + json);

            // Parse JSON thành List<Dictionary<string, string>> bằng Newtonsoft.Json
            var rows =
                JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);

            if (rows != null && rows.Count > 0)
            {
                var csvBuilder = new StringBuilder();

                // Lấy header từ dictionary đầu tiên
                var firstRow = rows[0];
                var headers  = firstRow.Keys.ToList();
                csvBuilder.AppendLine(string.Join(",", headers));

                // Ghi từng hàng
                foreach (var values in rows.Select(row => headers.Select(header => CsvHelper.EscapeCSV(row.GetValueOrDefault(header, "")))))
                {
                    csvBuilder.AppendLine(string.Join(",", values));
                }

                var csvData = csvBuilder.ToString();
                File.WriteAllText(assetPath, csvData, Encoding.UTF8);
                AssetDatabase.Refresh();
                Debug.Log("CSV data synced successfully!");
            }
            else
            {
                Debug.LogError("No data received from API.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error while fetching API: " + ex.Message);
        }
    }
}