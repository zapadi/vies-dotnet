/*
   Copyright 2017-2024 Adrian Popescu.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;

namespace Padi.Vies;

public struct ViesCheckVatResponse: IEquatable<ViesCheckVatResponse>
{
    public ViesCheckVatResponse(string countryCode, string vatNumber, DateTimeOffset requestDate, string name = null, string address = null, bool isValid = false) : this()
    {
        this.CountryCode = countryCode;
        this.VatNumber = vatNumber;
        this.RequestDate = requestDate;
        this.Name = name;
        this.Address = address;
        this.IsValid = isValid;
    }

    public string CountryCode { get; internal set; }
    public string VatNumber { get; internal set; }
    public DateTimeOffset RequestDate { get; internal set;}
    public string Name { get; internal set;}
    public string Address { get; internal set;}
    public bool IsValid { get; internal set;}

    public readonly bool Equals(ViesCheckVatResponse other)
    {
        return
            string.Equals(this.CountryCode, other.CountryCode, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(this.VatNumber, other.VatNumber, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(this.Address, other.Address, StringComparison.OrdinalIgnoreCase) && this.RequestDate.Equals(other.RequestDate) && this.IsValid == other.IsValid;
    }

    public readonly override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        return obj is ViesCheckVatResponse response && this.Equals(response);
    }

    public readonly override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (this.CountryCode != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.CountryCode) : 0);
            hashCode = (hashCode * 397) ^ (this.VatNumber != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.VatNumber) : 0);
            hashCode = (hashCode * 397) ^ this.RequestDate.GetHashCode();
            hashCode = (hashCode * 397) ^ (this.Name != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name) : 0);
            hashCode = (hashCode * 397) ^ (this.Address != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Address) : 0);
            hashCode = (hashCode * 397) ^ this.IsValid.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(ViesCheckVatResponse left, ViesCheckVatResponse right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ViesCheckVatResponse left, ViesCheckVatResponse right)
    {
        return !(left == right);
    }
}
