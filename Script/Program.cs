using System.Text;
using UnityEngine;

namespace ConsoleApplication4 {

    /// <summary>
    /// 顯示字串的 Unicode 編碼
    /// </summary>
    ///
    internal class Program {

        public static void Main(string args1, string args2) {
            StringBuilder str = new StringBuilder();
            foreach (var character in args1) {
                str.Append(GetEscapeSequence(character) + ",");
            }
            Debug.Log(str);

            str.Clear();
            foreach (var character in args2) {
                str.Append(GetEscapeSequence(character) + ",");
            }
            Debug.Log(str);
        }

        private static string GetEscapeSequence(char c) {
            return "\\u" + ((int)c).ToString("X4");
        }
    }
}