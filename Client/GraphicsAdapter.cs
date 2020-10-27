﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    //A - atvaizdo objektas (Bitmap)
    //B - spalvos objektas  (Color)
    interface GraphicsAdapter<A, B>
    {
        public void UnlockBits();
        public void LockBits();
        public void SetPixel(int x, int y, B color);
        public B[,] GetColorArray();
        public A GetImageCopy();
        public A GetImage();
        public int GetWidth();
        public int GetHeight();
        public void SetImage(A image);

    }
}
