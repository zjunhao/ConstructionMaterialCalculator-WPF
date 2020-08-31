using DecorationMaterialCalculator.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DecorationMaterialCalculator.Models
{
    public class InputItem
    {
        public InputItem()
        {

        }

        public InputItem(string productName, string borx, string type, string size, string position, string room, string unitLength = null, string applydirection = null)
        {
            ProductName = productName;
            BorX = borx;
            Type = type;
            Size = size;
            Position = position;
            Room = room;
            UnitLength = unitLength;
            ApplyDirection = applydirection;

            SummedUpSize = CalculationService.CalculateSummedUpSize(Size);
            Quantity = CalculationService.CalculateInputItemQuantity(this);
        }

        /// <summary>
        /// e.g. "s-802 锦绣纹"
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// e.g. value can be "b" (板) or "x" (线)
        /// </summary>
        public string BorX { get; set; }
        
        /// <summary>
        /// e.g. "300*3600" (In millimeter)
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Size in expression.
        /// e.g. "5.59+4.32+5.59+4.32-0.65" (In meter)
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// e.g. "顶","墙","顶角线","踢脚线"
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// e.g. "东南卧室","厨房"
        /// </summary>
        public string Room { get; set; }

        /// <summary>
        /// Used when BorX is b, which means board.
        /// e.g. "0.5" (In meter).
        /// If type is "300*3600" and UnitLength is "0.6", one such board can be divided to 3600 / (0.6 * 1000) = 6 pieces.
        /// null means no unit length.
        /// </summary>
        public string UnitLength { get; set; }

        /// <summary>
        /// Used when Position is "顶".
        /// e.g. "东西（铺贴）".
        /// null means no apply direction.
        /// </summary>
        public string ApplyDirection { get; set; }

        /// <summary>
        /// Sum up of expression in Size
        /// e.g. 15.92 (In meter)
        /// </summary>
        public double SummedUpSize { get; set; }
        
        /// <summary>
        /// Quantity needed for this ProductName and Type
        /// e.g. 19
        /// </summary>
        public int Quantity { get; set; }

    }
}
