﻿using System;

namespace IziHardGames.Projects.Sln
{
	public class SlnGlobal
    {
        private int start;
        private int end;
        private ReadOnlyMemory<char> mem;

        public string Value => mem.Span.ToString();
        public int Start => start;
        public int End => end;
        public int Length => end - start;

        public void SetStart(int i)
        {
            this.start = i;
        }
        public void SetEnd(int offset)
        {
            this.end = offset;
        }

        public void SetSlice(ReadOnlyMemory<char> readOnlyMemory)
        {
            this.mem = readOnlyMemory;
        }
    }
}