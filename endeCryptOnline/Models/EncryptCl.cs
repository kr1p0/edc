using System.Text;

namespace endeCryptOnline.Models
{
    public class EncryptCl
    {

        /*
        *remove everything before the first /
        input = input.Substring(input.IndexOf("/"));
        *remove everything after the first /
        input = input.Substring(0, input.IndexOf("/") + 1);
        *remove everything before the last /
        input = input.Substring(input.LastIndexOf("/"));
        *remove everything after the last /
        input = input.Substring(0, input.LastIndexOf("/") + 1);
        */

        public const string stringMarker = "@?♂??A<z???x?0??Q&c♣?2??6<??xF>GZ?HqcK??Q>2?KHq????Ap↨???X?mv3mLq?VzPCBe7E?xR" +
                             "??☺§?bW?Y§yg{? OY?GO??<zK?▬♂A ?Z?▬♫?♠???C S→[♣?k?F?♂?c??▲-? D^< q ?=? s ?d#$??)>!∟☻‼↓" +
                             "??U??GE??i?e?_?☼?↔;f??9G;f???¶:?Eg??▼?6-,Xda^fZ?fHv?h???►I??}i?r]?J♫Ajja*?Ka4?yK.?k?? 5♣" +
                             "?u?t.w?T♂.?ss?ZS??r☻am?RD?♠p♣t$P???o?RPO☻}?Nf??nU?Q?M???m???Ri6???]????XI▬⌂u>?S??lL?♥";   //length:335

        public static byte[] byteMarker = Encoding.UTF8.GetBytes(stringMarker);

        static public string extension;

        //static private string Index4password = "@kr#9y!z3v&ex";
        static private int[] Index4password = {5,64,18,333,10,4,43,68,12,87,31,280,11,88,19,98,122,131,
                                               301,331,299,2,77,21,56,306,145,42,11,321,307,58,67,81,22 }; //maks stringMarker.length :335

        //new
        public static string getRandomMarker(string marker)
        {
            var charListMarker = new List<char>(marker);
            var r = new Random();
            var newList = new List<char>(charListMarker.Count);
            while (charListMarker.Count > 0)
            {
                var i = r.Next(charListMarker.Count);
                newList.Add(charListMarker[i]);
                charListMarker.RemoveAt(i);
            }
            var result = new string(newList.ToArray());
            return result;
        }



        //prevent from going under 0 and over given length when adding "num"
        public static int preventOutOfRange2(int length, int num)
        {
            while (num >= length)
            {
                num -= length;
            }
            while (num < 0)
            {
                num += length;
            }
            return num;
        }


        // abcdefghijk  ->  key=e   ->   edfcgbhaikj  ...   
        public static byte[] newOrderedArray(byte[] arr, byte[] key) //used for 256 values of byte 0-255
        {
            int cycles = 0;

            while (cycles < key.Length)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    int k = 0;
                    int m = 1;
                    if (arr[i] == key[cycles])
                    {
                        byte[] tab2 = new byte[arr.Length];
                        for (int j = 0; j < arr.Length; j++, k++, m++)
                        {
                            tab2[j] = arr[preventOutOfRange2(arr.Length, i - k)];
                            if (j + 1 < arr.Length)
                            {
                                j++;
                                tab2[j] = arr[preventOutOfRange2(arr.Length, i + m)];
                            }
                        }
                        arr = tab2;
                        break;
                    }
                }
                cycles++;
            }
            return arr;

        }


        public static byte[] enDeCrypt(byte[] inputBytes, string key, string encryptOrDecrypt)
        {

            byte[] input = new byte[inputBytes.Length];
            inputBytes.CopyTo(input, 0);

            //create byte array 0-255
            byte[] arr = new byte[256];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = (byte)i;
            }

            // get key and string to encode
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            //byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);

            //get new sorted array
            arr = newOrderedArray(arr, keyBytes);


            //encrypted code i switch to left(decrypt right) by the length of key(password) based on new ordered array
            //prevent out of rang //zapobiea wyraczaniu poza granice tablice
            if (encryptOrDecrypt == "encrypt")
            {
                for (int i = 0; i < input.Length; i++)   //inputBytes.Length vs arr.length
                {
                    input[i] -= arr[preventOutOfRange2(arr.Length, keyBytes.Length)]; //255
                }
            }
            else if (encryptOrDecrypt == "decrypt")
            {

                for (int i = 0; i < input.Length; i++)
                {
                    input[i] += arr[preventOutOfRange2(arr.Length, keyBytes.Length)];
                }
            }

            return input;
        }



        public static byte[] enDeCryptimplementedKey(byte[] inputBytes, string key, string encryptOrDecrypt)
        {
            string randomMarker = getRandomMarker(stringMarker); //mixed up marker

            // byte[] marker = Encoding.UTF8.GetBytes(stringMarker); //################# MARKING
            byte[] marker = Encoding.UTF8.GetBytes(randomMarker); //#################get bytes of random marker 

            //reserved for keyLength information
            string keyArea = "00000000000000";

            if (encryptOrDecrypt == "encrypt")
            {

                //######test
                //testClass.findPassword(key4key, "in");

                //################# ading key length to key  area #####################
                string keyStringLength = key.Length.ToString();
                keyArea = keyArea.Substring(0, keyArea.Length - keyStringLength.Length);
                keyArea += keyStringLength;
                //adding key length information to key   
                string summedUpKey = keyArea + key;



                byte[] summedUpKeyBytes = Encoding.UTF8.GetBytes(summedUpKey);
                //encrypting key
                summedUpKeyBytes = enDeCrypt(summedUpKeyBytes, key, "encrypt");
                //encrypting inputBytes
                inputBytes = enDeCrypt(inputBytes, key, "encrypt");


                //######################adding key to inputBytes############################
                byte[] result;
                result = new byte[summedUpKeyBytes.Length + inputBytes.Length];
                summedUpKeyBytes.CopyTo(result, 0);
                inputBytes.CopyTo(result, summedUpKeyBytes.Length);


                //######################adding marker to inputBytes############################ MARKING
                byte[] result2;
                result2 = new byte[result.Length + marker.Length];
                marker.CopyTo(result2, 0);
                result.CopyTo(result2, marker.Length);
                return result2;
            }
            else if (encryptOrDecrypt == "decrypt")
            {


                //######################## removing marker ############################# DEMARKIN
                byte[] encryptedInputBytes = new byte[inputBytes.Length - marker.Length];
                for (int i = 0; i < inputBytes.Length - marker.Length; i++)
                {
                    encryptedInputBytes[i] = inputBytes[i + marker.Length];
                }






                //################################GETTING KEY#####################################
                //  byte[] encryptedInputBytes2 = new byte[encryptedInputBytes.Length];
                //   encryptedInputBytes.CopyTo(encryptedInputBytes2, 0);

                byte[] decrytpedInputBytes = enDeCrypt(encryptedInputBytes, key, "decrypt");
                string decrytpedInputString = Encoding.UTF8.GetString(decrytpedInputBytes);
                //################# isolating and decrytping the key length area ###############

                StringBuilder keyLengthIncluding0 = new StringBuilder();           //np.  000000007
                for (int i = 0; i < keyArea.Length; i++)
                {
                    keyLengthIncluding0.Append(decrytpedInputString[i]);

                }

                //##################  getting rid off keyLengthIncluding0 ####################
                decrytpedInputString = decrytpedInputString.Replace(keyLengthIncluding0.ToString(), string.Empty);


                //######################  getting rid off '0' #########################
                int keyLength;
                Int32.TryParse(keyLengthIncluding0.ToString(), out keyLength);     //np. 7


                //############# getting KeyString ###############
                StringBuilder readKey = new StringBuilder();

                for (int i = 0; i < keyLength; i++)
                {
                    readKey.Append(decrytpedInputString[i]);
                }

                string isolatedKeyString = readKey.ToString();
                //Console.WriteLine("Password read by decryption:  " + isolatedKeyString);     //READ KEY BY DECRYPTION




                //################## inputBytes with key and keyArea detached ###################
                byte[] shorteneddecrytpedInputBytes = new byte[encryptedInputBytes.Length - (keyArea.Length + keyLength)]; // np 00000000007  + snikers(length)
                for (int i = 0; i < shorteneddecrytpedInputBytes.Length; i++)
                {
                    shorteneddecrytpedInputBytes[i] = decrytpedInputBytes[i + keyArea.Length + keyLength];

                }
                if (key != isolatedKeyString)
                {
                    return null;
                }
                else
                {
                    return shorteneddecrytpedInputBytes;
                }
            }
            return null;
        }





        //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

        public static byte[] addFileExt(string filePath, byte[] inputBytes, string key)
        {
            //adds extension info to the end of encripted file
            FileInfo fi = new FileInfo(filePath);
            string ext = fi.Extension;
            byte[] extBytes = Encoding.UTF8.GetBytes(ext);

            //reserved for extensionLength information
            string extensionArea = "00000";


            //################# ading key length to key  area #####################
            string extensionStringLength = ext.Length.ToString();
            extensionArea = extensionArea.Substring(0, extensionArea.Length - extensionStringLength.Length);
            extensionArea += extensionStringLength;
            //adding extension length info to key   
            string summedUExt = ext + extensionArea;



            byte[] summedUExtBytes = Encoding.UTF8.GetBytes(summedUExt);
            //encrypting key
            summedUExtBytes = enDeCrypt(summedUExtBytes, key, "encrypt");


            //########### adding encrypted extension to the END of inputBytes ############
            byte[] result;
            result = new byte[summedUExtBytes.Length + inputBytes.Length];
            inputBytes.CopyTo(result, 0);
            summedUExtBytes.CopyTo(result, inputBytes.Length);

            return result;

        }

        //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        public static byte[] readRemoveFileExt(byte[] inputBytes, string key)
        {
            string extensionArea = "00000";


            //##################### Getting length of extension ###########################
            byte[] isolatedExtensionLength = new byte[extensionArea.Length];
            for (int i = 0; i < isolatedExtensionLength.Length; i++)
            {
                isolatedExtensionLength[i] = inputBytes[inputBytes.Length - extensionArea.Length + i];
            }
            isolatedExtensionLength = enDeCrypt(isolatedExtensionLength, key, "decrypt");
            string isolatedExtensionLengthString = Encoding.UTF8.GetString(isolatedExtensionLength);


            if (isolatedExtensionLengthString == extensionArea) //when there is no extension 
            {
                byte[] result2 = new byte[inputBytes.Length - extensionArea.Length];
                for (int i = 0; i < result2.Length; i++)
                {
                    result2[i] = inputBytes[i];
                }
                extension = "";
                return result2;
            }


            //###################### getting rid off '0' #########################
            for (int i = 0; i < isolatedExtensionLengthString.Length; i++)
            {
                if (isolatedExtensionLengthString[i] == '0')
                {
                    isolatedExtensionLengthString = isolatedExtensionLengthString.Substring(1);
                    i -= 1;
                }
                else
                {
                    break;
                }
            }
            int extLengthInt;
            Int32.TryParse(isolatedExtensionLengthString, out extLengthInt);
            //######################## getting extension ##########################
            byte[] extensionByte = new byte[extLengthInt];
            for (int i = 0; i < extensionByte.Length; i++)
            {
                extensionByte[i] = inputBytes[inputBytes.Length - extensionByte.Length - extensionArea.Length + i];
            }
            extensionByte = enDeCrypt(extensionByte, key, "decrypt");
            extension = Encoding.UTF8.GetString(extensionByte);


            //############################# Removing extension and extensionArea ######################################
            byte[] result = new byte[inputBytes.Length - extensionArea.Length - Int32.Parse(isolatedExtensionLengthString)];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = inputBytes[i];
            }
            return result;
        }





        public static bool checkIfEncrypted(byte[] inputBytes) //if saved and read characters in marcer area the same, the character is removed,
                                                               //when "markerList" == 0 ,file is encrypted
        {
            byte[] marker = Encoding.UTF8.GetBytes(stringMarker);
            List<byte> markerList = new List<byte>(marker);


            if (inputBytes.Length < marker.Length)
            {
                return false;
            }

            for (int i = 0; i < marker.Length; i++)
            {
                for (int j = 0; j < markerList.Count; j++)
                {
                    if (inputBytes[i] == markerList[j])
                    {
                        markerList.RemoveAt(j);
                        continue;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            if (markerList.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }




        public static string[] DirSearch(string sDir)
        {
            List<string> files = new List<string>();

            try
            {
                foreach (string f in Directory.GetFiles(sDir)) //pliki
                {
                    files.Add(f);
                }

                foreach (string d in Directory.GetDirectories(sDir)) //podkatalogi
                {
                    files.AddRange(DirSearch(d));
                }
            }
            catch (System.Exception excpt)
            {
                //Console.WriteLine(excpt.Message);
            }

            string[] array = files.ToArray();
            return array;
        }



    }
}
