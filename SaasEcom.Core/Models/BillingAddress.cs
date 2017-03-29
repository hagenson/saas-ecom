﻿using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaasEcom.Core.Models
{
    /// <summary>
    /// Class that represents a billing Address for a customer
    /// </summary>
    [ComplexType]
    public class BillingAddress: ICloneable
    {
        /// <summary>
        /// Gets or sets the name of the person / company.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Display(ResourceType = typeof (Resources.SaasEcom), Name = "BillingAddress_Name_Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the address line1.
        /// </summary>
        /// <value>
        /// The address line1.
        /// </value>
        [Display(ResourceType = typeof (Resources.SaasEcom), Name = "BillingAddress_AddressLine1_Address_1")]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets the address line2.
        /// </summary>
        /// <value>
        /// The address line2.
        /// </value>
        [Display(ResourceType = typeof (Resources.SaasEcom), Name = "BillingAddress_AddressLine2_Address_2")]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [Display(ResourceType = typeof (Resources.SaasEcom), Name = "BillingAddress_City_City")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [Display(ResourceType = typeof (Resources.SaasEcom), Name = "BillingAddress_State_State")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        /// <value>
        /// The zip code.
        /// </value>
        [Display(ResourceType = typeof (Resources.SaasEcom), Name = "BillingAddress_ZipCode_Zip_Code")]
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [Display(ResourceType = typeof (Resources.SaasEcom), Name = "BillingAddress_Country_Country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the vat.
        /// </summary>
        /// <value>
        /// The vat.
        /// </value>
        [Display(ResourceType = typeof (Resources.SaasEcom), Name = "BillingAddress_Vat_VAT_Number")]
        public string Vat { get; set; }

        public object Clone()
        {
            BillingAddress result = new BillingAddress
            {
                AddressLine1 = this.AddressLine1,
                AddressLine2 = this.AddressLine2,
                City = this.City,
                Country = this.Country,
                Name = this.Name,
                State = this.State,
                Vat = this.Vat,
                ZipCode = this.ZipCode
            };
            return result;
        }

        public override string ToString()
        {
            string[] lines = new string[]
            {
                Name,
                AddressLine1,
                AddressLine2,
                City,
                State,
                ZipCode,
                Country
            };

            if (lines.Any(x => !String.IsNullOrEmpty(x)))
                return lines
                    .Where(x => !String.IsNullOrWhiteSpace(x))
                    .Aggregate((a, b) => a + Environment.NewLine + b);
            else
                return null;
        }
    }
}
