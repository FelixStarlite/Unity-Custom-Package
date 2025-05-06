using System;
using System.Collections;
using UnityEngine;

namespace Starlite
{
    /// <summary>
    /// 啟動行動裝置的GPS定位
    /// </summary>
    public class GpsManager : MonoBehaviour {
        public static double latitude;     //經度
        public static double longitude;    //緯度
        private void Start() {
            StartCoroutine(StartLocationService());
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 啟動 GPS 服務
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartLocationService()  {
            if (!Input.location.isEnabledByUser) {
                Debug.Log("Location services are not enabled by the user.");
                yield break;
            }

            Input.location.Start();

            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            if (maxWait <= 0) {
                Debug.Log("Timed out");
                yield break;
            }

            if (Input.location.status == LocationServiceStatus.Failed) {
                Debug.Log("Unable to determine device location");
            } else {
                InvokeRepeating(nameof(UpdateGpsData), 0, 1f);
            }
        }

        /// <summary>
        /// 更新 GPS 資料
        /// </summary>
        private void UpdateGpsData() {
            if (Input.location.status == LocationServiceStatus.Running) {
                latitude = Input.location.lastData.latitude;
                longitude = Input.location.lastData.longitude;

                Debug.Log($"x:" + latitude + ", y:" + longitude);
            } else {
                Debug.Log("Location services are not running.");
            }
        }

        /// <summary>
        /// 計算當前座標和目標座標的距離
        /// </summary>
        /// <param name="target_X"></param>
        /// <param name="target_Y"></param>
        /// <returns></returns>
        public static double DistanceTo(double target_X, double target_Y) {
            const double R = 6371000; // 地球平均半徑(公尺)
            var dLat = (float)(latitude - target_X) * Mathf.Deg2Rad;
            var dLon = (float)(longitude - target_Y) * Mathf.Deg2Rad;
            var a =
                Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                Mathf.Cos((float)target_X * Mathf.Deg2Rad) *
                Mathf.Cos((float)latitude * Mathf.Deg2Rad) *
                Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
            var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
            return (float)(R * c);
        }
    }
}
