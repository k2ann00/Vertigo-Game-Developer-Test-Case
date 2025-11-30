#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WheelOfFortune.Data;

namespace WheelOfFortune.Editor
{
    public class ZoneConfigGenerator : EditorWindow
    {
        private const string ZONE_CONFIGS_PATH = "Assets/_Project/Resources/ZoneConfigs";
        private const string ITEM_DATA_PATH = "Assets/_Project/Resources/Items";

        private int startZone = 1;
        private int endZone = 100;
        private bool overwriteExisting = false;
        private Vector2 scrollPosition;

        [MenuItem("Tools/WheelOfFortune/Zone Config Generator")]
        public static void ShowWindow()
        {
            ZoneConfigGenerator window = GetWindow<ZoneConfigGenerator>("Zone Config Generator by k2ann00");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Header
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Zone Configuration Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "30 tane teker teker ZoneConfig dosyası oluşturmamak için vitesi bir üste çıkarttı ama burası da 400 satır oldu .d.\n" +
                "Zone tipi (Bronze/Silver/Golden) otomatik belirlenir.",
                MessageType.Info);

            GUILayout.Space(10);

            // Settings
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            startZone = EditorGUILayout.IntField("Start Zone", startZone);
            endZone = EditorGUILayout.IntField("End Zone", endZone);
            overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);

            GUILayout.Space(10);

            // Validation
            if (startZone < 1)
            {
                startZone = 1;
            }

            if (endZone < startZone)
            {
                endZone = startZone;
            }

            // Info
            EditorGUILayout.HelpBox(
                $"Toplam {endZone - startZone + 1} zone config oluşturulacak.\n" +
                $"Path: {ZONE_CONFIGS_PATH}",
                MessageType.None);

            GUILayout.Space(10);

            // Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate All Zones", GUILayout.Height(30)))
            {
                GenerateZones(startZone, endZone);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Quick presets
            EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Zones 1-100"))
            {
                startZone = 1;
                endZone = 100;
            }

            if (GUILayout.Button("Zones 1-30"))
            {
                startZone = 1;
                endZone = 30;
            }

            if (GUILayout.Button("Zones 1-10"))
            {
                startZone = 1;
                endZone = 10;
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Zone Type Info
            EditorGUILayout.LabelField("Zone Type Rules", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "• Bronze (Normal): Tüm zonlar\n" +
                "• Silver (Safe): Her 5. zone (5, 10, 15, 20, 25...)\n" +
                "• Golden (Super): Her 30. zone (30, 60, 90...)\n\n" +
                "Safe ve Super zone'larda bomba yoktur.",
                MessageType.Info);

            GUILayout.Space(10);

            // Folder Actions
            EditorGUILayout.LabelField("Folder Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create Folders"))
            {
                CreateNecessaryFolders();
            }

            if (GUILayout.Button("Clear All Configs"))
            {
                if (EditorUtility.DisplayDialog(
                    "Clear Configs",
                    "Tüm zone config'leri silmek istediğinize emin misiniz?",
                    "Evet, Sil",
                    "İptal"))
                {
                    ClearAllConfigs();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void GenerateZones(int start, int end)
        {
            // Klasörü oluştur
            CreateNecessaryFolders();

            int createdCount = 0;
            int skippedCount = 0;

            EditorUtility.DisplayProgressBar("Generating Zones", "Creating zone configs...", 0f);

            for (int zoneNumber = start; zoneNumber <= end; zoneNumber++)
            {
                float progress = (float)(zoneNumber - start) / (end - start + 1);
                EditorUtility.DisplayProgressBar(
                    "Generating Zones",
                    $"Creating Zone {zoneNumber}...",
                    progress);

                bool created = CreateZoneConfig(zoneNumber, overwriteExisting);

                if (created)
                {
                    createdCount++;
                }
                else
                {
                    skippedCount++;
                }
            }

            EditorUtility.ClearProgressBar();

            // Refresh
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Result
            EditorUtility.DisplayDialog(
                "Zone Generation Complete",
                $"✓ Created: {createdCount} configs\n" +
                $"○ Skipped: {skippedCount} configs\n\n" +
                $"Path: {ZONE_CONFIGS_PATH}",
                "OK");

            Debug.Log($"[ZoneConfigGenerator] Generated {createdCount} zone configs! (Skipped: {skippedCount})");
        }

        private bool CreateZoneConfig(int zoneNumber, bool overwrite)
        {
            string fileName = $"ZoneConfig_{zoneNumber:D3}.asset";
            string fullPath = Path.Combine(ZONE_CONFIGS_PATH, fileName);

            // Varsa skip (overwrite false ise)
            if (!overwrite && File.Exists(fullPath))
            {
                return false;
            }

            // Config oluştur
            ZoneWheelConfig config = ScriptableObject.CreateInstance<ZoneWheelConfig>();

            // Zone numarasını set et (OnValidate otomatik çalışacak)
            SerializedObject serializedConfig = new SerializedObject(config);
            serializedConfig.FindProperty("zoneNumber").intValue = zoneNumber;
            serializedConfig.ApplyModifiedProperties();

            // Zone tipini belirle (manuel olarak da yapabiliriz)
            SetupZoneType(config, zoneNumber);

            // Multiplier'ları ayarla
            SetupMultipliers(config, zoneNumber);

            // Item pool'u ayarla
            SetupItemPool(config, zoneNumber);

            // Asset olarak kaydet
            AssetDatabase.CreateAsset(config, fullPath);

            return true;
        }

        private void SetupZoneType(ZoneWheelConfig config, int zoneNumber)
        {
            SerializedObject serializedConfig = new SerializedObject(config);

            if (zoneNumber % 30 == 0)
            {
                // Super Zone
                serializedConfig.FindProperty("wheelType").enumValueIndex = (int)WheelType.Golden;
                serializedConfig.FindProperty("isSuperZone").boolValue = true;
                serializedConfig.FindProperty("isSafeZone").boolValue = true;
                serializedConfig.FindProperty("bombCount").intValue = 0;
            }
            else if (zoneNumber % 5 == 0)
            {
                // Safe Zone
                serializedConfig.FindProperty("wheelType").enumValueIndex = (int)WheelType.Silver;
                serializedConfig.FindProperty("isSafeZone").boolValue = true;
                serializedConfig.FindProperty("isSuperZone").boolValue = false;
                serializedConfig.FindProperty("bombCount").intValue = 0;
            }
            else
            {
                // Normal Zone
                serializedConfig.FindProperty("wheelType").enumValueIndex = (int)WheelType.Bronze;
                serializedConfig.FindProperty("isSafeZone").boolValue = false;
                serializedConfig.FindProperty("isSuperZone").boolValue = false;
                serializedConfig.FindProperty("bombCount").intValue = 1;
            }

            serializedConfig.ApplyModifiedProperties();
        }

        private void SetupMultipliers(ZoneWheelConfig config, int zoneNumber)
        {
            SerializedObject serializedConfig = new SerializedObject(config);

            // Zone ilerledikçe ödüller artar
            float cashMultiplier = 1f + (zoneNumber * 0.1f);
            float goldMultiplier = 1f + (zoneNumber * 0.05f);

            serializedConfig.FindProperty("cashMultiplier").floatValue = cashMultiplier;
            serializedConfig.FindProperty("goldMultiplier").floatValue = goldMultiplier;

            serializedConfig.ApplyModifiedProperties();
        }

        private void SetupItemPool(ZoneWheelConfig config, int zoneNumber)
        {
            // Item Data asset'lerini yükle
            List<ItemData> allItems = LoadAllItemData();

            if (allItems.Count == 0)
            {
                Debug.LogWarning($"[ZoneConfigGenerator] Zone {zoneNumber}: Item Data bulunamadı! " +
                                 $"Önce ItemData asset'leri oluşturun.");
                return;
            }

            // Zone'da kullanılabilir item'ları filtrele
            List<ItemData> availableItems = new List<ItemData>();
            foreach (var item in allItems)
            {
                if (item.IsAvailableInZone(zoneNumber))
                {
                    availableItems.Add(item);
                }
            }

            // Config'e ata
            SerializedObject serializedConfig = new SerializedObject(config);
            SerializedProperty itemsProperty = serializedConfig.FindProperty("availableItems");

            itemsProperty.ClearArray();
            for (int i = 0; i < availableItems.Count; i++)
            {
                itemsProperty.InsertArrayElementAtIndex(i);
                itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue = availableItems[i];
            }

            serializedConfig.ApplyModifiedProperties();
        }

        private List<ItemData> LoadAllItemData()
        {
            List<ItemData> items = new List<ItemData>();

            // ItemData klasöründeki tüm asset'leri bul
            string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { ITEM_DATA_PATH });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);

                if (item != null && item.IsValid())
                {
                    items.Add(item);
                }
            }

            return items;
        }

        private void CreateNecessaryFolders()
        {
            // ZoneConfigs klasörü
            if (!AssetDatabase.IsValidFolder(ZONE_CONFIGS_PATH))
            {
                string parentFolder = "Assets/_Project/Data";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets/_Project", "Data");
                }
                AssetDatabase.CreateFolder(parentFolder, "ZoneConfigs");
                Debug.Log($"[ZoneConfigGenerator] Created folder: {ZONE_CONFIGS_PATH}");
            }

            // Items klasörü
            if (!AssetDatabase.IsValidFolder(ITEM_DATA_PATH))
            {
                string parentFolder = "Assets/_Project/Data";
                AssetDatabase.CreateFolder(parentFolder, "Items");
                Debug.Log($"[ZoneConfigGenerator] Created folder: {ITEM_DATA_PATH}");
            }

            AssetDatabase.Refresh();
        }

        private void ClearAllConfigs()
        {
            if (!Directory.Exists(ZONE_CONFIGS_PATH))
            {
                Debug.LogWarning("[ZoneConfigGenerator] Zone configs folder does not exist!");
                return;
            }

            string[] files = Directory.GetFiles(ZONE_CONFIGS_PATH, "*.asset");
            int deletedCount = 0;

            foreach (string file in files)
            {
                AssetDatabase.DeleteAsset(file);
                deletedCount++;
            }

            AssetDatabase.Refresh();

            Debug.Log($"[ZoneConfigGenerator] Deleted {deletedCount} zone configs.");
            EditorUtility.DisplayDialog("Configs Cleared", $"Deleted {deletedCount} zone configs.", "OK");
        }
    }
}
#endif
