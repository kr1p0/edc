namespace endeCryptOnline.Models
{
    public class StringEncryptCl
    {

        public static string stringMarker = "#>@ASCŚu~GÓhBsWÓxzaLbcPjUśm7~WPcghkÓBrh{|}RWF;s|fQdB~rh{,LEelŻlpegż5ęjt5nfAo#qśŚ;:Ź@Ź0F@";
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


        public static string newOrderedStringArray(char[] arr, string charZero)
        {
            // charZero definiuje pierwszy znak, nastepny znak to pierwszy od prawej, 
            // nastepny pierwszy od lewej, iteracyjnie.
            // Sortowanie zachodzi rekurencyjnie, odpowiednio do ilosci znakow charZero

            char[] tab = arr;
            char[] tab2 = new char[tab.Length];
            charZero.ToCharArray();

            for (int i = 0; i < tab.Length; i++)
            {
                int k = 0;
                int m = 1;
                if (tab[i] == charZero[0])
                {
                    for (int j = 0; j < tab.Length; j++, k++, m++)
                    {
                        tab2[j] = tab[preventOutOfRange2(tab.Length, i - k)];
                        if (j + 1 < tab.Length)
                        {
                            j++;
                            tab2[j] = tab[preventOutOfRange2(tab.Length, i + m)];
                        }
                    }
                    tab = tab2;
                    break;
                }
            }

            if (charZero.Length > 1)
            {
                charZero = charZero.Substring(1);
                return newOrderedStringArray(tab, charZero);
            }
            else
            {
                string result = new string(tab);
                return result;
            }

            //result= result.Substring(1); //deletes first character
            //result = string.Concat(charZero, result);
            //string output = "kryspin";
            //string firstChar = output.Substring(0, 1); //get the first character
            //string restOutput = output.Substring(1, output.Length - 1); //get the first character
        }



        public static string stringEnDeCrypt(string input, string key, string encryptORdecrypt)
        {



            char getRandomChar(char[] array)
            {
                //   kod losuje znak( wsrod znakow dostepnych w tablicy) od ktorej bedzie sie zaczynac nowa tablica,
                Random rnd = new Random();
                int randomIndex = rnd.Next(0, array.Length - 1);  // creates a number between 0 and arr length
                return array[randomIndex];

            }






            char[] alphabet = { '?','a', 'b','~','X', 'B','1', 'Y','d','`', 'L','f','!', 'g', 'h','_', 'G','2','@','W', 'k','=','ł','#', 'm', 'n','o','ö',
                                'p','$', 'r','3','T','ß', 's','Ą','%','ś','t','^','u', 'ó', 'w','4','&', 'y','ą', 'z','F', 'ż','*','Ę','R', 'ź','5','x','(', 'v', 'q','+',
                                'A','-', 'i','÷', 'C','{','D', 'j','6','.','E','[','Ö', 'H', 'I','>', 'J','7',']', 'K','Ł', 'M','}', 'N','O','|',' ','Ä','ü',
                                'P','8',':', 'S',',','Ś',';', 'c','U','9', 'l','Ü', 'Ó','"', 'Z','<', 'Ż','e', 'Ź','/','0', 'V','ę', 'Q',')','ä'};
            /*
          char[] alphabet = { '?','a','Ã','b','~','X','B','Ë','1','Y','d','¨','`','ø',
                              'L','f','!','g','h','ª','ÿ','þ','ý',
                              '_','G','Â','2','@','W','k','Ê','=','ł','#', 'm','n','o','¯','®','¬','«','´',
                              'ś','t','^','u', 'ó','w','4','&','y','÷','ö','õ','ô','û','ú','ù',
                              'ą','z','F','À','ż','*','(','v','q','+','©','¢','µ','æ',
                              'A','-','i','¿','€','C','{','D','j','Ç','6','.','E','[','H','I','>','£','¶',
                              'J','7',']','¾','K','Ł','M','}','N','Æ','O','|','²',' ','ò','ñ','ð',
                              'P','8',':','½','S',',','Ś','l','Ó','"','º','¹','¸','·',
                              'Z','<','¼','Ż','e','Ź','/','0','V','ę','Ä','Q',')','Ć','ń','¡','³','±','°',
                              '»','Ö','Õ','Ô','Ò','Ñ','Ð','Ï','Î','Í','Ę','R','È','ź','5','x','Ì',
                              'å','ä','ã','â','á','à','ß','s','Ą','É','%','ï','î','í','ì',
                              'Þ','Ý',';','c','Å','U','9','Ü','Û','Ú','Ù',
                              'Ø','×','ć','Ń','§','¦','¥','¤','p','$',
                              'Á','r','3','T','ë','ê','é','è','ç'};

           */


            alphabet = newOrderedStringArray(alphabet, key).ToCharArray();  //defined character

            //alphabet = newOrderedArray(alphabet, getRandomChar(alphabet).ToString()).ToCharArray(); //random character


            char[] characters = input.ToCharArray();
            int[] index = new int[characters.Length];
            int shift = characters.Length;
            //gets the char that will start the char array 'alpabet'
            char charZeroAlphabet = getRandomChar(alphabet);
            //gets new sorted array with 'charZeroAlphabet' as starting point
            // alphabet = newOrderedArray(alphabet, charZeroAlphabet).ToCharArray();


            if (encryptORdecrypt == "encrypt")
            {
                //################moves characters to righht##################
                for (int i = 0; i < characters.Length; i++)
                {
                    for (int j = 0; j < alphabet.Length; j++)
                    {
                        if (characters[i] == alphabet[j])
                        {
                            // shift++;
                            characters[i] = alphabet[preventOutOfRange2(alphabet.Length, j + shift)];
                            break;
                        }
                    }
                }
            }

            //########################################################################################

            if (encryptORdecrypt == "decrypt")
            {
                //################moves characters to left####################
                for (int i = 0; i < characters.Length; i++)
                {
                    for (int j = 0; j < alphabet.Length; j++)
                    {
                        if (characters[i] == alphabet[j])
                        {
                            //shift++;
                            characters[i] = alphabet[preventOutOfRange2(alphabet.Length, j - shift)];
                            break;
                        }
                    }
                }
            }

            string output = new string(characters);
            return output;
        }


        public static string stringEnDeCryptAddeddMarker(string input, string key, string encryptORdecrypt)
        {

            string newOrderedMarker = newOrderedStringArray(stringMarker.ToCharArray(), key);
            if (encryptORdecrypt == "encrypt")
            {

                string encryptedInput = stringEnDeCrypt(input, key, "encrypt");
                string inputAddedMarker = newOrderedMarker + encryptedInput;
                return inputAddedMarker;
            }
            if (encryptORdecrypt == "decrypt")
            {
                input = input.Substring(newOrderedMarker.Length);
                string decryptedInput = stringEnDeCrypt(input, key, "decrypt");
                return decryptedInput;
            }
            return null;

        }


        public static bool checkIfEncrypted(string input)
        {
            string tempMarker = stringMarker;
            if (input.Length < stringMarker.Length)
            {
                return false;
            }

            for (int i = 0; i < stringMarker.Length; i++)
            {
                for (int j = 0; j < tempMarker.Length; j++)
                {
                    if (input[i] == tempMarker[j])
                    {
                        tempMarker = tempMarker.Replace(tempMarker[j].ToString(), "");
                        if (tempMarker.Length == 0)
                        {
                            return true;
                        }
                        break;
                    }
                }

            }
            return false;
        }


    }
}
