/*
   Copyright 2017 Adrian Popescu.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the spevatic language governing permissions and
   limitations under the License.
*/

/* This is based on work from http://www.braemoor.co.uk/software/vat.shtml */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Zapadi.Vies
{
    public static class VatValidation
    {
        private const string SPACE_STRING = " ";
        private const string DASH_STRING = "-";

        private static readonly Regex countryCodeRegex =
            new Regex("[a-zA-Z]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Dictionary<string, ValidationTuple> countryValidationTuples =
            new Dictionary<string, ValidationTuple>
            {
                {"AT", new ValidationTuple(@"^U[A-Z]\d{8}$", VatValidation.AT)},
                {"BE", new ValidationTuple(@"^0?\d{9}$", VatValidation.BE)},
                {"BG", new ValidationTuple(@"^\d{9,10}$", VatValidation.BG)},
                {"CY", new ValidationTuple(@"^([0-59]\d{7}[A-Z])$", VatValidation.CY)},
                {"CZ", new ValidationTuple(@"^(\d{8,10})(\d{3})?$", VatValidation.CZ)},
                {"DE", new ValidationTuple(@"^[1-9]\d{8}$", VatValidation.DE)},
                {"DK", new ValidationTuple(@"^(\d{2} ?){3}\d{2}$", VatValidation.DK)},
                {"EE", new ValidationTuple(@"^10\d{7}$", VatValidation.EE)},
                {"EL", new ValidationTuple(@"^\d{9}$", VatValidation.EL)},
                {
                    "ES",
                    new ValidationTuple(@"^([A-Z]\d{8})|([A-HN-SW]\d{7}[A-J])|([0-9YZ]\d{7}[A-Z])|([KLMX]\d{7}[A-Z])$",
                        VatValidation.ES)
                },
                {"FI", new ValidationTuple(@"^\d{8}$", VatValidation.FI)},
                {
                    "FR",
                    new ValidationTuple(@"^(\d{11})|([A-HJ-NP-Z]\d{10})|(\d[A-HJ-NP-Z]\d{9})|([A-HJ-NP-Z]{2}\d{9})$",
                        VatValidation.FR)
                },
                {"GB", new ValidationTuple(@"^\d{9}|\d{12}|(GD|HA)\d{3}$", VatValidation.GB)},
                {"HR", new ValidationTuple(@"^\d{11}$", VatValidation.HR)},
                {"HU", new ValidationTuple(@"^\d{8}$", VatValidation.HU)},
                {
                    "IE",
                    new ValidationTuple(@"^(\d{7}[A-W])|([7-9][A-Z\*\+)]\d{5}[A-W])|(\d{7}[A-W][AH])$",
                        VatValidation.IE)
                },
                {"IT", new ValidationTuple(@"^\d{11}$", VatValidation.IT)},
                {"LT", new ValidationTuple(@"^(\d{9}|\d{12})$", VatValidation.LT)},
                {"LU", new ValidationTuple(@"^\d{8}$", VatValidation.LU)},
                {"LV", new ValidationTuple(@"^\d{11}$", VatValidation.LV)},
                {"MT", new ValidationTuple(@"^[1-9]\d{7}$", VatValidation.MT)},
                {"NL", new ValidationTuple(@"^\d{9}B\d{2}$", VatValidation.NL)},
                {"PL", new ValidationTuple(@"^\d{10}$", VatValidation.PL)},
                {"PT", new ValidationTuple(@"^\d{9}$", VatValidation.PT)},
                {"RO", new ValidationTuple(@"^[1-9]\d{1,9}$", VatValidation.RO)},
                {"SE", new ValidationTuple(@"^\d{10}01$", VatValidation.SE)},
                {"SI", new ValidationTuple(@"^[1-9]\d{7}$", VatValidation.SI)},
                {"SK", new ValidationTuple(@"^[1-9]\d[2346-9]\d{7}$", VatValidation.SK)}
            };

        public static string Sanitize(string vatNumber)
        {
            return vatNumber
                .Replace(SPACE_STRING, string.Empty)
                .Replace(DASH_STRING, string.Empty)
                .Trim()
                .ToUpperInvariant()
                .Replace("GR", "EL");
        }

        public static Tuple<string,string> ParseVat(string vatNumber)
        {
            if (string.IsNullOrWhiteSpace(vatNumber))
                throw new ViesValidationException("VAT number cannot be null");

            vatNumber = VatValidation.Sanitize(vatNumber);

            if (vatNumber.Length < 3)
                throw new ViesValidationException($"Vat number '{vatNumber}' is too short.");

            var countryCode = vatNumber.Substring(0, 2);
            var number = vatNumber.Substring(2);

            VatValidation.EnsureCountryCodeIsValid(countryCode);
            VatValidation.EnsureVatNumberIsValid(countryCode, number);
            
            return new Tuple<string, string>(countryCode, number);
        }
        
        private static void EnsureCountryCodeIsValid(string countryCode)
        {
            if (!countryCodeRegex.IsMatch(countryCode))
            {
                throw new ViesValidationException($"{countryCode} is not a valid ISO_3166-1 country code.");
            }
            if (!countryValidationTuples.ContainsKey(countryCode))
            {
                throw new ViesValidationException($"{countryCode} is not a european member state.");
            }
        }

        private static void EnsureVatNumberIsValid(string countryCode, string vatNumber)
        {
            if (!countryValidationTuples[countryCode].VatRegex.IsMatch(vatNumber))
                throw new ViesValidationException(
                    $"'{countryCode}' does not match the countries VAT ID specifications.'");

            if (!countryValidationTuples[countryCode].VatCheckDigit.Invoke(vatNumber))
                throw new ViesValidationException(
                    $"'{countryCode}' does not match the countries VAT ID specifications.'");
        }


        public static bool EL(string vat)
        {
            if (vat.Length == 8) vat = "0" + vat;

            var i = 0;
            var sum = new[] {256, 128, 64, 32, 16, 8, 4, 2}.Sum(m => vat[i++].ToInt() * m);

            var checkDigit = sum % 11;
            if (checkDigit > 9) checkDigit = 0;

            return checkDigit == vat[8].ToInt();
        }

        public static bool AT(string vat)
        {
            var i = 1;
            var sum = new[] {1, 2, 1, 2, 1, 2, 1}
                .Select(m => vat[i++].ToInt() * m)
                .Select(temp => temp > 9 ? (int) Math.Floor(temp / 10D) + temp % 10 : temp)
                .Sum();

            var checkDigit = 10 - (sum + 4) % 10;
            if (checkDigit == 10) checkDigit = 0;

            return checkDigit == vat[8].ToInt();
        }

        public static bool BE(string vat)
        {
            // First character of 10 digit numbers should be 0
            if (vat.Length == 10 && vat[0] != '0') return false;

            // Nine digit numbers have a 0 inserted at the front.
            if (vat.Length == 9) vat = "0" + vat;

            // Modulus 97 check on last nine digits
            return 97 - int.Parse(vat.Substring(0, 8)) % 97 == int.Parse(vat.Substring(8, 2));
        }

        public static bool BG(string vat)
        {
            if (vat.Length == 9)
            {
                return BG9DigitsVat(vat);
            }
            // 10 digit VAT code - see if it relates to a standard physical person
            if (BGPhysicalPerson(vat))
            {
                return true;
            }
            if (BGForeignerPhysicalPerson(vat))
            {
                return true;
            }
            return BGMiscellaneousVATNumber(vat);
        }


        // var i = 0;
        // var sum = new[] { 4, 3, 2, 7, 6, 5, 4, 3, 2 }.Sum(m => vat[i++].ToInt() * m) % 11;

        // var checkDigit = sum;
        // if (checkDigit == 10)
        // {
        //     i = 0;
        //     checkDigit = new[] { 3, 4, 5, 6, 7, 8, 9, 10 }.Sum(m => vat[i++].ToInt() * m) % 11;
        //     if (checkDigit == 10) checkDigit = 0;
        // }

        // return checkDigit == vat[9].ToInt();
        // }
        //}

        private static bool BG9DigitsVat(string vat)
        {
            var total = 0;
            // First try to calculate the check digit using the first multipliers  
            var temp = 0;
            for (var i = 0; i < 8; i++) temp += vat[i].ToInt() * (i + 1);

            // See if we have a check digit yet
            total = temp % 11;
            if (total != 10)
            {
                return total == vat[8].ToInt();
            }
            // We got a modulus of 10 before so we have to keep going. Calculate the new check digit using  
            // the different multipliers  
            temp = 0;
            for (var index = 0; index < 8; index++) temp += vat[index].ToInt() * (index + 3);

            // See if we have a check digit yet. If we still have a modulus of 10, set it to 0.
            total = temp % 11;
            if (total == 10) total = 0;
            return total == vat[8].ToInt();
        }

        private static bool BGPhysicalPerson(string vat)
        {
            if (Regex.IsMatch(@"^\d\d[0-5]\d[0-3]\d\d{4}$", vat))
            {
                // Check month
                var month = int.Parse(vat.Substring(2, 2));
                if (month > 0 && month < 13 || month > 20 && month < 33 || month > 40 && month < 53)
                {
                    // Extract the next digit and multiply by the counter.
                    var multipliers = new[] {2, 4, 8, 5, 10, 9, 7, 3, 6};
                    var total = 0;
                    for (var i = 0; i < 9; i++) total += vat[i].ToInt() * multipliers[i];

                    // Establish check digit.
                    total = total % 11;
                    if (total == 10) total = 0;

                    // Check to see if the check digit given is correct, If not, try next type of person
                    if (total == vat[9].ToInt()) return true;
                }
            }
            return false;
        }

        private static bool BGForeignerPhysicalPerson(string vat)
        {
            var multipliers = new[] {21, 19, 17, 13, 11, 9, 7, 3, 1};
            var total = 0;
            for (var i = 0; i < 9; i++) total += vat[i] * multipliers[i];

            // Check to see if the check digit given is correct, If not, try next type of person
            return total % 10 == vat[9].ToInt();
        }

        private static bool BGMiscellaneousVATNumber(string vat)
        {
            var multipliers = new[] {4, 3, 2, 7, 6, 5, 4, 3, 2};
            var total = 0;
            for (var i = 0; i < 9; i++) total += vat[i].ToInt() * multipliers[i];

            // Establish check digit.
            total = 11 - total % 11;
            switch (total)
            {
                case 10:
                    return false;
                case 11:
                    total = 0;
                    break;
            }

            // Check to see if the check digit given is correct, If not, we have an error with the VAT number
            return total == vat[9].ToInt();
        }

        public static bool CY(string vat)
        {
            if (int.Parse(vat.Substring(0, 2)) == 12) return false;

            var result = 0;
            for (var i = 0; i < 8; i++)
            {
                var temp = vat[i].ToInt();
                if (i % 2 == 0)
                {
                    switch (temp)
                    {
                        case 0:
                            temp = 1;
                            break;
                        case 1:
                            temp = 0;
                            break;
                        case 2:
                            temp = 5;
                            break;
                        case 3:
                            temp = 7;
                            break;
                        case 4:
                            temp = 9;
                            break;
                        default:
                            temp = temp * 2 + 3;
                            break;
                    }
                }
                result = result + temp;
            }

            var checkDigit = result % 26;
            return vat[8] == (char) (checkDigit + 65);
        }

        public static bool CZ(string vat)
        {
            // Only do check digit validation for standard VAT numbers
            if (vat.Length != 8) return true;

            var i = 0;
            var sum = new[] {8, 7, 6, 5, 4, 3, 2}.Sum(m => vat[i++].ToInt() * m);

            var checkDigit = 11 - sum % 11;
            if (checkDigit == 10) checkDigit = 0;
            if (checkDigit == 11) checkDigit = 1;

            return checkDigit == vat[7].ToInt();
        }

        public static bool DE(string vat)
        {
            var product = 10;
            for (var i = 0; i < 8; i++)
            {
                // Extract the next digit and implement perculiar algorithm!.
                var sum = (vat[i].ToInt() + product) % 10;
                if (sum == 0)
                {
                    sum = 10;
                }
                product = 2 * sum % 11;
            }

            var checkDigit = 11 - product == 10 ? 0 : 11 - product;

            return checkDigit == vat[8].ToInt();
        }

        public static bool DK(string vat)
        {
            var i = 0;
            var sum = new[] {2, 7, 6, 5, 4, 3, 2, 1}.Sum(m => vat[i++].ToInt() * m);

            return sum % 11 == 0;
        }

        public static bool EE(string vat)
        {
            var i = 0;
            var sum = new[] {3, 7, 1, 3, 7, 1, 3, 7}.Sum(m => vat[i++].ToInt() * m);

            var checkDigit = 10 - sum % 10;
            if (checkDigit == 10) checkDigit = 0;

            return checkDigit == vat[8].ToInt();
        }

        public static bool ES(string vat)
        {
            return false;
        }

        public static bool FI(string vat)
        {
            var i = 0;
            var sum = new[] {7, 9, 10, 5, 8, 4, 2}.Sum(m => vat[i++].ToInt() * m);

            var checkDigit = 11 - sum % 11;
            if (checkDigit > 9)
            {
                checkDigit = 0;
            }

            return checkDigit == vat[7].ToInt();
        }

        public static bool FR(string vat)
        {
            int temp;
            if (!int.TryParse(vat.Substring(0, 2), out temp)) return true;

            // Extract the last nine digits as an integer.
            var val = int.Parse(vat.Substring(2));

            var checkDigit = (val * 100 + 12) % 97;

            return checkDigit == temp;
        }

        public static bool GB(string vat)
        {
            var multipliers = new[] {8, 7, 6, 5, 4, 3, 2};

            string prefix = vat.Substring(0, 2);
            int no;
            // Government departments
            if (prefix == "GD")
            {
                if (int.TryParse(vat.Substring(2, 3), out no))
                {
                    return no < 500;
                }
            }

            // Health authorities
            if (prefix == "HA")
            {
                if (int.TryParse(vat.Substring(2, 3), out no))
                {
                    return no > 499;
                }
                return false;
            }

            // Standard and commercial numbers
            var total = 0;

            // 0 VAT numbers disallowed!
            if (vat[0].ToInt() == 0) return false;

            // Check range is OK for modulus 97 calculation
            no = int.Parse(vat.Substring(0, 7));

            // Extract the next digit and multiply by the counter.
            for (var i = 0; i < 7; i++) total += vat[i] * multipliers[i];

            // Old numbers use a simple 97 modulus, but new numbers use an adaptation of that (less 55). Our 
            // VAT number could use either system, so we check it against both.

            // Establish check digits by subtracting 97 from total until negative.
            var cd = total;
            while (cd > 0)
            {
                cd = cd - 97;
            }

            // Get the absolute value and compare it with the last two characters of the VAT number. If the 
            // same, then it is a valid traditional check digit. However, even then the number must fit within
            // certain specified ranges.
            cd = Math.Abs(cd);
            if (cd == int.Parse(vat.Substring(7, 9)) && no < 9990001 && (no < 100000 || no > 999999) &&
                (no < 9490001 || no > 9700000)) return true;

            // Now try the new method by subtracting 55 from the check digit if we can - else add 42
            if (cd >= 55)
                cd = cd - 55;
            else
                cd = cd + 42;
            return cd == int.Parse(vat.Substring(7, 9)) && no > 1000000;
        }

        public static bool HR(string vat)
        {
            var product = 10;

            for (var i = 0; i < 10; i++)
            {
                int sum = (vat[i].ToInt() + product) % 10;
                if (sum == 0)
                {
                    sum = 10;
                }
                product = 2 * sum % 11;
            }

            var checkdigit = (product + vat[10].ToInt()) % 10;
            return checkdigit == 1;
        }

        public static bool HU(string vat)
        {
            var i = 0;
            var sum = new[] {9, 7, 3, 1, 9, 7, 3}.Sum(m => vat[i++].ToInt() * m);

            var checkDigit = 10 - sum % 10;
            if (checkDigit == 10) checkDigit = 0;

            return checkDigit == vat[7].ToInt();
        }

        public static bool IE(string vat)
        {
            if (Regex.IsMatch(vat, @"/^\d[A-Z\*\+]/"))
            {
                vat = "0" + vat.Substring(2, 7) + vat.Substring(0, 1) + vat.Substring(7, 8);
            }

            var i = 0;
            var sum = new[] {8, 7, 6, 5, 4, 3, 2}.Sum(m => vat[i++].ToInt() * m);

            // If the number is type 3 then we need to include the trailing A or H in the calculation
            if (Regex.IsMatch(@"/^\d{7}[A-Z][AH]$/", vat))
            {
                // Add in a multiplier for the character A (1*9=9) or H (8*9=72)
                if (vat[8] == 'H')
                    sum += 72;
                else
                    sum += 9;
            }

            var checkDigit = sum % 23;
            return vat[7] == (checkDigit == 0 ? 'W' : (char) (checkDigit + 64));
        }

        public static bool IT(string vat)
        {
            // The last three digits are the issuing office, and cannot exceed more 201
            if (int.Parse(vat.Substring(0, 7)) == 0) return false;

            var temp = int.Parse(vat.Substring(7, 3));

            if ((temp < 1 || temp > 201) && temp != 999 && temp != 888) return false;

            var i = 0;
            var sum = 0;
            foreach (var m in new[] {1, 2, 1, 2, 1, 2, 1, 2, 1, 2})
            {
                temp = vat[i++].ToInt() * m;
                sum += temp > 9 ? (int) Math.Floor(temp / 10D) + temp % 10 : temp;
            }

            var checkDigit = 10 - sum % 10;
            if (checkDigit > 9)
            {
                checkDigit = 0;
            }

            return checkDigit == vat[10].ToInt();
        }

        public static bool LT(string vat)
        {
            // 9 character VAT numbers are for legal persons
            if (vat.Length == 9)
            {
                if (vat[8] != '1') return false;
                var sum = 0;
                for (var i = 0; i < 8; i++) sum += vat[i].ToInt() * (i + 1);

                var checkDigit = sum % 11;
                // Can have a double check digit calculation!
                if (checkDigit == 10)
                {
                    var i = 0;
                    checkDigit = new[] {3, 4, 5, 6, 7, 8, 9, 1}.Sum(m => vat[i++].ToInt() * m);
                }

                if (checkDigit == 10)
                {
                    checkDigit = 0;
                }

                return checkDigit == vat[8].ToInt();
            }
            return LTTemporarilyRegisteredTaxpayers(vat);
        }

        private static bool LTTemporarilyRegisteredTaxpayers(string vat)
        {
            // 11th character must be one
            if (vat[11] != '1') return false;

            int i = 0;
            // Extract the next digit and multiply by the counter+1.
            var total = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 1, 2}.Sum(m => vat[i++].ToInt() * m);

            // Can have a double check digit calculation!
            if (total % 11 == 10)
            {
                i = 0;
                total = new[] {3, 4, 5, 6, 7, 8, 9, 1, 2, 3, 4}.Sum(m => vat[i++].ToInt() * m);
            }

            // Establish check digit.
            total = total % 11;
            if (total == 10)
            {
                total = 0;
            }
            ;

            // Compare it with the last character of the VAT number. If it's the same, then it's valid.
            if (total == vat[11].ToInt())
                return true;
            return false;
        }

        public static bool LU(string vat)
        {
            return int.Parse(vat.Substring(0, 6)) % 89 == int.Parse(vat.Substring(6, 2));
        }

        public static bool LV(string vat)
        {
            // Only check the legal bodies
            if (Regex.IsMatch(vat, "/^[0-3]/"))
            {
                return Regex.IsMatch(vat, "^[0-3][0-9][0-1][0-9]");
            }
            var i = 0;
            var sum = new[] {9, 1, 4, 8, 3, 10, 2, 5, 7, 6}.Sum(m => vat[i++].ToInt() * m);

            var checkDigit = sum % 11;

            if (checkDigit == 4 && vat[0] == '9')
            {
                checkDigit = checkDigit - 45;
            }

            if (checkDigit == 4)
            {
                checkDigit = 4 - checkDigit;
            }
            else
            {
                if (checkDigit > 4)
                {
                    checkDigit = 14 - checkDigit;
                }
                else
                {
                    if (checkDigit < 4)
                    {
                        checkDigit = 3 - checkDigit;
                    }
                }
            }

            return checkDigit == vat[10].ToInt();
        }

        public static bool MT(string vat)
        {
            var i = 0;
            var sum = new[] {3, 4, 6, 7, 8, 9}.Sum(m => vat[i++].ToInt() * m);

            var checkDigit = 37 - sum % 37;

            return checkDigit == int.Parse(vat.Substring(6, 2));
        }

        public static bool NL(string vat)
        {
            var i = 0;
            var sum = new[] {9, 8, 7, 6, 5, 4, 3, 2}.Sum(m => vat[i++].ToInt() * m);

            var checkDigit = sum % 11;
            if (checkDigit > 9)
            {
                checkDigit = 0;
            }

            return checkDigit == vat[8].ToInt();
        }

        public static bool NO(string vat)
        {
            // See http://www.brreg.no/english/coordination/number.html

            var total = 0;
            var multipliers = new[] {3, 2, 7, 6, 5, 4, 3, 2};

            // Extract the next digit and multiply by the counter.
            for (var i = 0; i < 8; i++) total += vat[i].ToInt() * multipliers[i];

            // Establish check digits by getting modulus 11. Check digits > 9 are invalid
            total = 11 - total % 11;
            if (total == 11)
            {
                total = 0;
            }
            if (total < 10)
            {
                return total == vat[8].ToInt();
            }

            return false;
        }

        public static bool PL(string vat)
        {
            var i = 0;
            var sum = new[] {6, 5, 7, 2, 3, 4, 5, 6, 7}.Sum(m => vat[i++].ToInt() * m);

            var checkDigit = sum % 11;
            if (checkDigit > 9)
            {
                checkDigit = 0;
            }

            return checkDigit == vat[9].ToInt();
        }

        public static bool PT(string vat)
        {
            var i = 0;
            var sum = new[] {9, 8, 7, 6, 5, 4, 3, 2}.Sum(m => vat[i++].ToInt() * m);

            var checkDigit = 11 - sum % 11;
            if (checkDigit > 9)
            {
                checkDigit = 0;
            }
            return checkDigit == vat[8].ToInt();
        }

        public static bool SE(string vat)
        {
            var i = 0;
            var sum = new[] {2, 1, 2, 1, 2, 1, 2, 1, 2}
                .Select(m => vat[i++].ToInt() * m)
                .Select(temp => temp > 9 ? (int) Math.Floor(temp / 10D) + temp % 10 : temp)
                .Sum();

            var checkDigit = 10 - sum % 10;
            if (checkDigit == 10) checkDigit = 0;

            return checkDigit == vat[9].ToInt();
        }

        public static bool SK(string vat)
        {
            //            var i = 3;
            //            var sum = new[] {8, 7, 6, 5, 4, 3, 2}.Sum(m => vat[i++].ToInt() * m);
            //
            //            var checkDigit = 11 - sum % 11;
            //            if (checkDigit > 9) checkDigit = checkDigit - 10;
            //
            //            return checkDigit == vat[9].ToInt();

            int nr;
            if (int.TryParse(vat, out nr))
            {
                return nr % 11 == 0;
            }
            return false;
        }

        public static bool SI(string vat)
        {
            var i = 0;
            var sum = new[] {8, 7, 6, 5, 4, 3, 2}.Sum(m => vat[i++].ToInt() * m);

            var checkDigit = 11 - sum % 11;
            if (checkDigit > 9)
            {
                checkDigit = 0;
            }

            return checkDigit == vat[7].ToInt();
        }

        public static bool RO(string vat)
        {
            int nr;
            int.TryParse(vat, out nr);
            var control = nr % 10;

            vat = vat.Substring(0, vat.Length - 1);
            while (vat.Length < 9) vat = "0" + vat;

            var i = 0;
            var sum = new[] {7, 5, 3, 2, 1, 7, 5, 3, 2}.Sum(m => vat[i++].ToInt() * m);

            var checkDigit = sum * 10 % 11;
            if (checkDigit == 10) checkDigit = 0;

            return checkDigit == control;
        }
    }
}