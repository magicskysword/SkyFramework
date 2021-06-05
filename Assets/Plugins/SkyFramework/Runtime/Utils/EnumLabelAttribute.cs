using System;
using UnityEngine;

namespace SkyFrameWork
{
    public class EnumLabelAttribute : HeaderAttribute
    {
        public Type enumType;
        
        public EnumLabelAttribute(string header,Type enumType) : base(header)
        {
            this.enumType = enumType;
        }
    }
}