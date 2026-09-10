using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace CodeMonkey.FreeWindow {

    [CreateAssetMenu()]
    public class CodeMonkeyFreeSO : ScriptableObject {


        private const long SECONDS_BETWEEN_CONTACTING_WEBSITE = 3600;


        private static CodeMonkeyFreeSO codeMonkeyFreeSO;


        public static CodeMonkeyFreeSO GetCodeMonkeyFreeSO() {
            if (codeMonkeyFreeSO != null) {
                return codeMonkeyFreeSO;
            }

            // ค้นหาเฉพาะ Asset ที่เป็น ScriptableObject ชนิด CodeMonkeyFreeSO (ข้ามไฟล์ .cs)
            string[] codeMonkeyFreeSOGuidArray = AssetDatabase.FindAssets($"t:{nameof(CodeMonkeyFreeSO)}");

            foreach (string codeMonkeyFreeSOGuid in codeMonkeyFreeSOGuidArray) {
                string codeMonkeyFreeSOPath = AssetDatabase.GUIDToAssetPath(codeMonkeyFreeSOGuid);

                // ป้องกันการโหลดไฟล์สคริปต์ C# หรือไฟล์ชนิดอื่นที่ไม่ใช่ .asset
                if (!codeMonkeyFreeSOPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                codeMonkeyFreeSO = AssetDatabase.LoadAssetAtPath<CodeMonkeyFreeSO>(codeMonkeyFreeSOPath);
                if (codeMonkeyFreeSO != null) {
                    return codeMonkeyFreeSO;
                }
            }

            // Fallback (Q2 Option A): หากหาไฟล์ .asset ไม่พบ ให้สร้าง Instance ชั่วคราวในหน่วยความจำ เพื่อให้ Editor ทำงานต่อได้โดยไม่เกิด NRE
            if (codeMonkeyFreeSO == null) {
                codeMonkeyFreeSO = ScriptableObject.CreateInstance<CodeMonkeyFreeSO>();
                codeMonkeyFreeSO.currentVersion = "1.01";
                codeMonkeyFreeSO.subtype = "kitchenchaos";
            }

            return codeMonkeyFreeSO;
        }



        public string currentVersion;
        public string subtype;
        public long lastShownTimestamp;

        [SerializeField] private LastUpdateResponse lastUpdateResponse;
        [SerializeField] private long checkedLastUpdateTimestamp;
        [SerializeField] private LastQOTDResponse lastQOTDResponse;
        [SerializeField] private long lastQotdTimestamp;
        [SerializeField] private LastDynamicHeaderResponse lastDynamicHeaderResponse;
        [SerializeField] private long lastDynamicHeaderTimestamp;
        [SerializeField] private WebsiteLatestMessage websiteLatestMessage;
        [SerializeField] private long websiteLatestMessageTimestamp;
        [SerializeField] private LatestVideos websiteLatestVideos;
        [SerializeField] private long websiteLatestVideosTimestamp;




        [Serializable]
        private struct GenericActionJSONData {
            public string at;
            public string st;
        }

        [Serializable]
        private struct WebsiteResponse {
            public int returnCode;
            public string returnText;
        }

        [Serializable]
        private struct WebsiteResponse<T> {
            public int returnCode;
            public T returnText;
        }

        [Serializable]
        public struct LastUpdateResponse {
            public string version;
            public string versionUrl;
        }

        public static void CheckForUpdates(Action<LastUpdateResponse> onFoundUpdate) {
            CodeMonkeyFreeSO codeMonkeyInteractiveSO = GetCodeMonkeyFreeSO();
            if (codeMonkeyInteractiveSO == null) return;

            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (codeMonkeyInteractiveSO.lastUpdateResponse.version == null || codeMonkeyInteractiveSO.lastUpdateResponse.version == "") {
                codeMonkeyInteractiveSO.lastUpdateResponse.version = codeMonkeyInteractiveSO.currentVersion;
            }

            long secondsBetweenCheckingForUpdates = 3600;
            if (unixTimestamp - codeMonkeyInteractiveSO.checkedLastUpdateTimestamp < secondsBetweenCheckingForUpdates) {
                // Too soon
                onFoundUpdate?.Invoke(codeMonkeyInteractiveSO.lastUpdateResponse);
                return;
            }

            // Enough time has passed to check for update
            codeMonkeyInteractiveSO.checkedLastUpdateTimestamp = unixTimestamp;
            EditorUtility.SetDirty(codeMonkeyInteractiveSO);

            string url = "https://unitycodemonkey.com/generic_action_json.php";
            UnityWebRequest unityWebRequest = new UnityWebRequest(url, "POST");
            unityWebRequest.timeout = 5;

            string jsonData = JsonUtility.ToJson(new GenericActionJSONData {
                at = "editorwindowversion",
                st = codeMonkeyInteractiveSO.subtype,
            });
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            unityWebRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            unityWebRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");

            unityWebRequest.SendWebRequest().completed += (AsyncOperation asyncOperation) => {
                try {
                    UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = asyncOperation as UnityWebRequestAsyncOperation;

                    if (unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.ConnectionError ||
                        unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.DataProcessingError ||
                        unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.ProtocolError) {
                        // Error
                        //onError(unityWebRequest.error);
                        onFoundUpdate?.Invoke(codeMonkeyInteractiveSO.lastUpdateResponse);
                    } else {
                        string downloadText = unityWebRequest.downloadHandler.text;
                        WebsiteResponse websiteResponse = JsonUtility.FromJson<WebsiteResponse>(downloadText);
                        if (websiteResponse.returnCode == 1) {
                            // Success
                            LastUpdateResponse lastUpdateResponse = JsonUtility.FromJson<LastUpdateResponse>(websiteResponse.returnText);
                            codeMonkeyInteractiveSO.lastUpdateResponse = lastUpdateResponse;
                            onFoundUpdate?.Invoke(codeMonkeyInteractiveSO.lastUpdateResponse);
                        } else {
                            // Something went wrong
                            onFoundUpdate?.Invoke(codeMonkeyInteractiveSO.lastUpdateResponse);
                        }
                    }
                } catch (Exception) {
                    onFoundUpdate?.Invoke(codeMonkeyInteractiveSO.lastUpdateResponse);
                }
                unityWebRequest.Dispose();
            };
        }

        [Serializable]
        public struct LastQOTDResponse {
            public string questionId;
            public string questionText;
            public string answerA;
            public string answerB;
            public string answerC;
            public string answerD;
            public string answerE;
        }

        public static void GetLastQOTD(Action<LastQOTDResponse> onResponse) {
            CodeMonkeyFreeSO codeMonkeyInteractiveSO = GetCodeMonkeyFreeSO();
            if (codeMonkeyInteractiveSO == null) return;

            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            long secondsBetweenCheckingForUpdates = SECONDS_BETWEEN_CONTACTING_WEBSITE;
            if (unixTimestamp - codeMonkeyInteractiveSO.lastQotdTimestamp < secondsBetweenCheckingForUpdates) {
                // Too soon
                onResponse?.Invoke(codeMonkeyInteractiveSO.lastQOTDResponse);
                return;
            }

            // Enough time has passed to check for update
            codeMonkeyInteractiveSO.lastQotdTimestamp = unixTimestamp;
            EditorUtility.SetDirty(codeMonkeyInteractiveSO);

            string url = "https://unitycodemonkey.com/generic_action_json.php";
            UnityWebRequest unityWebRequest = new UnityWebRequest(url, "POST");
            unityWebRequest.timeout = 5;

            string jsonData = JsonUtility.ToJson(new GenericActionJSONData {
                at = "getLastQotd",
            });
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            unityWebRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            unityWebRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");

            unityWebRequest.SendWebRequest().completed += (AsyncOperation asyncOperation) => {
                try {
                    UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = asyncOperation as UnityWebRequestAsyncOperation;

                    if (unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.ConnectionError ||
                        unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.DataProcessingError ||
                        unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.ProtocolError) {
                        // Error
                        //onError(unityWebRequest.error);
                        onResponse?.Invoke(codeMonkeyInteractiveSO.lastQOTDResponse);
                    } else {
                        string downloadText = unityWebRequest.downloadHandler.text;
                        WebsiteResponse websiteResponse = JsonUtility.FromJson<WebsiteResponse>(downloadText);
                        if (websiteResponse.returnCode == 1) {
                            // Success
                            LastQOTDResponse lastQOTDResponse = JsonUtility.FromJson<LastQOTDResponse>(websiteResponse.returnText);
                            codeMonkeyInteractiveSO.lastQOTDResponse = lastQOTDResponse;
                            onResponse?.Invoke(codeMonkeyInteractiveSO.lastQOTDResponse);
                        } else {
                            // Something went wrong
                            onResponse?.Invoke(codeMonkeyInteractiveSO.lastQOTDResponse);
                        }
                    }
                } catch (Exception) {
                    onResponse?.Invoke(codeMonkeyInteractiveSO.lastQOTDResponse);
                }
                unityWebRequest.Dispose();
            };
        }

        [Serializable]
        public struct LastDynamicHeaderResponse {
            public string topImageUrl;
            public string topText;
            public string topLink;
            public string bottomText;
            public string bottomLink;
        }

        public static void GetLastDynamicHeader(Action<LastDynamicHeaderResponse> onResponse) {
            CodeMonkeyFreeSO codeMonkeyInteractiveSO = GetCodeMonkeyFreeSO();
            if (codeMonkeyInteractiveSO == null) return;

            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            long secondsBetweenCheckingForUpdates = SECONDS_BETWEEN_CONTACTING_WEBSITE;
            if (unixTimestamp - codeMonkeyInteractiveSO.lastDynamicHeaderTimestamp < secondsBetweenCheckingForUpdates) {
                // Too soon
                onResponse?.Invoke(codeMonkeyInteractiveSO.lastDynamicHeaderResponse);
                return;
            }

            // Enough time has passed to check for update
            codeMonkeyInteractiveSO.lastDynamicHeaderTimestamp = unixTimestamp;
            EditorUtility.SetDirty(codeMonkeyInteractiveSO);

            string url = "https://unitycodemonkey.com/generic_action_json.php";
            UnityWebRequest unityWebRequest = new UnityWebRequest(url, "POST");
            unityWebRequest.timeout = 5;

            string jsonData = JsonUtility.ToJson(new GenericActionJSONData {
                at = "getDynamicEmailHeaderJson",
            });
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            unityWebRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            unityWebRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");

            unityWebRequest.SendWebRequest().completed += (AsyncOperation asyncOperation) => {
                try {
                    UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = asyncOperation as UnityWebRequestAsyncOperation;

                    if (unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.ConnectionError ||
                        unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.DataProcessingError ||
                        unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.ProtocolError) {
                        // Error
                        //onError(unityWebRequest.error);
                        onResponse?.Invoke(codeMonkeyInteractiveSO.lastDynamicHeaderResponse);
                    } else {
                        string downloadText = unityWebRequest.downloadHandler.text;
                        WebsiteResponse websiteResponse = JsonUtility.FromJson<WebsiteResponse>(downloadText);
                        if (websiteResponse.returnCode == 1) {
                            // Success
                            LastDynamicHeaderResponse lastDynamicHeaderResponse = 
                                JsonUtility.FromJson<LastDynamicHeaderResponse>(websiteResponse.returnText);
                            codeMonkeyInteractiveSO.lastDynamicHeaderResponse = lastDynamicHeaderResponse;
                            onResponse?.Invoke(codeMonkeyInteractiveSO.lastDynamicHeaderResponse);
                        } else {
                            // Something went wrong
                            onResponse?.Invoke(codeMonkeyInteractiveSO.lastDynamicHeaderResponse);
                        }
                    }
                } catch (Exception) {
                    onResponse?.Invoke(codeMonkeyInteractiveSO.lastDynamicHeaderResponse);
                }
                unityWebRequest.Dispose();
            };
        }



        [Serializable]
        public struct WebsiteLatestMessage {
            public string text;
        }

        public static void GetLatestMessage(Action<WebsiteLatestMessage> onResponse) {
            CodeMonkeyFreeSO codeMonkeyInteractiveSO = GetCodeMonkeyFreeSO();
            if (codeMonkeyInteractiveSO == null) return;

            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            long secondsBetweenCheckingForUpdates = SECONDS_BETWEEN_CONTACTING_WEBSITE;
            if (unixTimestamp - codeMonkeyInteractiveSO.websiteLatestMessageTimestamp < secondsBetweenCheckingForUpdates) {
                // Too soon
                onResponse?.Invoke(codeMonkeyInteractiveSO.websiteLatestMessage);
                return;
            }

            // Enough time has passed to check for update
            codeMonkeyInteractiveSO.websiteLatestMessageTimestamp = unixTimestamp;
            EditorUtility.SetDirty(codeMonkeyInteractiveSO);

            string url = "https://unitycodemonkey.com/generic_action_json.php";
            UnityWebRequest unityWebRequest = new UnityWebRequest(url, "POST");
            unityWebRequest.timeout = 5;

            string jsonData = JsonUtility.ToJson(new GenericActionJSONData {
                at = "editorwindowlatestMessage",
                st = codeMonkeyInteractiveSO.subtype,
            });
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            unityWebRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            unityWebRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");

            unityWebRequest.SendWebRequest().completed += (AsyncOperation asyncOperation) => {
                try {
                    UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = asyncOperation as UnityWebRequestAsyncOperation;

                    if (unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.ConnectionError ||
                        unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.DataProcessingError ||
                        unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.ProtocolError) {
                        // Error
                        //onError(unityWebRequest.error);
                        onResponse?.Invoke(codeMonkeyInteractiveSO.websiteLatestMessage);
                    } else {
                        string downloadText = unityWebRequest.downloadHandler.text;
                        WebsiteResponse websiteResponse = JsonUtility.FromJson<WebsiteResponse>(downloadText);
                        if (websiteResponse.returnCode == 1) {
                            // Success
                            WebsiteLatestMessage websiteLatestMessage = JsonUtility.FromJson<WebsiteLatestMessage>(websiteResponse.returnText);
                            codeMonkeyInteractiveSO.websiteLatestMessage = websiteLatestMessage;
                            onResponse?.Invoke(codeMonkeyInteractiveSO.websiteLatestMessage);
                        } else {
                            // Something went wrong
                            onResponse?.Invoke(codeMonkeyInteractiveSO.websiteLatestMessage);
                        }
                    }
                } catch (Exception) {
                    onResponse?.Invoke(codeMonkeyInteractiveSO.websiteLatestMessage);
                }
                unityWebRequest.Dispose();
            };
        }





        [Serializable]
        public class LatestVideos {
            public LatestVideoSingle[] videos;
        }

        [Serializable]
        public class LatestVideoSingle {
            public string youTubeId;
            public string title;
        }

        public static void GetWebsiteLatestVideos(Action<LatestVideos> onResponse) {
            CodeMonkeyFreeSO codeMonkeyInteractiveSO = GetCodeMonkeyFreeSO();
            if (codeMonkeyInteractiveSO == null) return;

            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            long secondsBetweenCheckingForUpdates = SECONDS_BETWEEN_CONTACTING_WEBSITE;
            if (unixTimestamp - codeMonkeyInteractiveSO.websiteLatestVideosTimestamp < secondsBetweenCheckingForUpdates) {
                // Too soon
                onResponse?.Invoke(codeMonkeyInteractiveSO.websiteLatestVideos);
                return;
            }

            // Enough time has passed to check for update
            codeMonkeyInteractiveSO.websiteLatestVideosTimestamp = unixTimestamp;
            EditorUtility.SetDirty(codeMonkeyInteractiveSO);

            string url = "https://unitycodemonkey.com/generic_action_json.php";
            UnityWebRequest unityWebRequest = new UnityWebRequest(url, "POST");
            unityWebRequest.timeout = 5;

            string jsonData = JsonUtility.ToJson(new GenericActionJSONData {
                at = "getLastVideos",
            });
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            unityWebRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            unityWebRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");

            unityWebRequest.SendWebRequest().completed += (AsyncOperation asyncOperation) => {
                try {
                    UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = asyncOperation as UnityWebRequestAsyncOperation;

                    if (unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.ConnectionError ||
                        unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.DataProcessingError ||
                        unityWebRequestAsyncOperation.webRequest.result == UnityWebRequest.Result.ProtocolError) {
                        // Error
                        //onError(unityWebRequest.error);
                        onResponse?.Invoke(codeMonkeyInteractiveSO.websiteLatestVideos);
                    } else {
                        string downloadText = unityWebRequest.downloadHandler.text;
                        WebsiteResponse websiteResponse = JsonUtility.FromJson<WebsiteResponse>(downloadText);
                        if (websiteResponse.returnCode == 1) {
                            // Success
                            LatestVideos websiteLatestVideos = JsonUtility.FromJson<LatestVideos>(websiteResponse.returnText);
                            codeMonkeyInteractiveSO.websiteLatestVideos = websiteLatestVideos;
                            onResponse?.Invoke(codeMonkeyInteractiveSO.websiteLatestVideos);
                        } else {
                            // Something went wrong
                            onResponse?.Invoke(codeMonkeyInteractiveSO.websiteLatestVideos);
                        }
                    }
                } catch (Exception) {
                    onResponse?.Invoke(codeMonkeyInteractiveSO.websiteLatestVideos);
                }
                unityWebRequest.Dispose();
            };
        }


    }

}