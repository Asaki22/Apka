//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace App.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Wypalanie
    {
        public int ProduktID { get; set; }
        public Nullable<int> IloscWykonana { get; set; }
        public Nullable<int> CzasWypalania { get; set; }
        public string Wypalono { get; set; }
    
        public virtual Produkty Produkty { get; set; }
    }
}
