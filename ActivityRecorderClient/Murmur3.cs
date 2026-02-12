using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient
{
	//original from: http://blog.teamleadnet.com/2012/08/murmurhash3-ultra-fast-hash-algorithm.html
	//https://github.com/flier/pyfasthash/blob/master/src/MurmurHash/MurmurHash3.cpp
	//https://code.google.com/p/smhasher/source/browse/trunk/MurmurHash3.cpp
	public class Murmur3 //_x64_128
	{
		// 128 bit output, 64 bit platform version

		public static byte[] ComputeHash(byte[] bytes)
		{
			return new Murmur3().ComputeHashImpl(bytes);
		}

		public static readonly ulong ReadSize = 16;
		private static ulong C1 = 0x87c37b91114253d5L;
		private static ulong C2 = 0x4cf5ad432745937fL;

		private ulong length;
		private readonly uint seed;
		ulong h1;
		ulong h2;

		private Murmur3()
			: this(0)
		{
		}

		private Murmur3(uint seed)
		{
			this.seed = seed;
		}

		private void MixBody(ulong k1, ulong k2)
		{
			h1 ^= MixKey1(k1);

			h1 = RotateLeft(h1, 27);
			h1 += h2;
			h1 = h1 * 5 + 0x52dce729;

			h2 ^= MixKey2(k2);

			h2 = RotateLeft(h2, 31);
			h2 += h1;
			h2 = h2 * 5 + 0x38495ab5;
		}

		private static ulong MixKey1(ulong k1)
		{
			k1 *= C1;
			k1 = RotateLeft(k1, 31);
			k1 *= C2;
			return k1;
		}

		private static ulong MixKey2(ulong k2)
		{
			k2 *= C2;
			k2 = RotateLeft(k2, 33);
			k2 *= C1;
			return k2;
		}

		private static ulong MixFinal(ulong k)
		{
			// avalanche bits

			k ^= k >> 33;
			k *= 0xff51afd7ed558ccdL;
			k ^= k >> 33;
			k *= 0xc4ceb9fe1a85ec53L;
			k ^= k >> 33;
			return k;
		}

		private byte[] ComputeHashImpl(byte[] bb)
		{
			ProcessBytes(bb);

			// finalization

			h1 ^= length;
			h2 ^= length;

			h1 += h2;
			h2 += h1;

			h1 = Murmur3.MixFinal(h1);
			h2 = Murmur3.MixFinal(h2);

			h1 += h2;
			h2 += h1;

			var hash = new byte[Murmur3.ReadSize];

			Array.Copy(BitConverter.GetBytes(h1), 0, hash, 0, 8);
			Array.Copy(BitConverter.GetBytes(h2), 0, hash, 8, 8);

			return hash;
		}

		private void ProcessBytes(byte[] bb)
		{
			h1 = seed;
			this.length = 0L;

			int pos = 0;
			ulong remaining = (ulong)bb.Length;

			// read 128 bits, 16 bytes, 2 longs in eacy cycle
			while (remaining >= ReadSize)
			{
				ulong k1 = GetUInt64(bb, pos);
				pos += 8;

				ulong k2 = GetUInt64(bb, pos);
				pos += 8;

				length += ReadSize;
				remaining -= ReadSize;

				MixBody(k1, k2);
			}

			// tail - if the input MOD 16 != 0
			if (remaining > 0)
			{
				ProcessBytesRemaining(bb, remaining, pos);
			}
		}

		private void ProcessBytesRemaining(byte[] bb, ulong remaining, int pos)
		{
			ulong k1 = 0;
			ulong k2 = 0;
			length += remaining;

			// little endian (x86) processing
			switch (remaining)
			{
				case 15:
					k2 ^= (ulong)bb[pos + 14] << 48; // fall through
					goto case 14;
				case 14:
					k2 ^= (ulong)bb[pos + 13] << 40; // fall through
					goto case 13;
				case 13:
					k2 ^= (ulong)bb[pos + 12] << 32; // fall through
					goto case 12;
				case 12:
					k2 ^= (ulong)bb[pos + 11] << 24; // fall through
					goto case 11;
				case 11:
					k2 ^= (ulong)bb[pos + 10] << 16; // fall through
					goto case 10;
				case 10:
					k2 ^= (ulong)bb[pos + 9] << 8; // fall through
					goto case 9;
				case 9:
					k2 ^= (ulong)bb[pos + 8]; // fall through
					goto case 8;
				case 8:
					k1 ^= GetUInt64(bb, pos);
					break;
				case 7:
					k1 ^= (ulong)bb[pos + 6] << 48; // fall through
					goto case 6;
				case 6:
					k1 ^= (ulong)bb[pos + 5] << 40; // fall through
					goto case 5;
				case 5:
					k1 ^= (ulong)bb[pos + 4] << 32; // fall through
					goto case 4;
				case 4:
					k1 ^= (ulong)bb[pos + 3] << 24; // fall through
					goto case 3;
				case 3:
					k1 ^= (ulong)bb[pos + 2] << 16; // fall through
					goto case 2;
				case 2:
					k1 ^= (ulong)bb[pos + 1] << 8; // fall through
					goto case 1;
				case 1:
					k1 ^= (ulong)bb[pos]; // fall through
					break;
				default:
					Debug.Fail("Something went wrong with remaining bytes calculation.");
					break;
			}

			h1 ^= MixKey1(k1);
			h2 ^= MixKey2(k2);
		}

		private static ulong RotateLeft(ulong original, int bits)
		{
			return (original << bits) | (original >> (64 - bits));
		}

		private static ulong RotateRight(ulong original, int bits)
		{
			return (original >> bits) | (original << (64 - bits));
		}

		private static ulong GetUInt64(byte[] bb, int pos)
		{
			return (ulong)BitConverter.ToInt64(bb, pos);
		}
	}

	//public static class IntHelpers
	//{
	//	public static ulong RotateLeft(this ulong original, int bits)
	//	{
	//		return (original << bits) | (original >> (64 - bits));
	//	}

	//	public static ulong RotateRight(this ulong original, int bits)
	//	{
	//		return (original >> bits) | (original << (64 - bits));
	//	}

	//	unsafe public static ulong GetUInt64(this byte[] bb, int pos)
	//	{
	//		// we only read aligned longs, so a simple casting is enough
	//		fixed (byte* pbyte = &bb[pos])
	//		{
	//			return *((ulong*)pbyte);
	//		}
	//	}
	//}

	/*
	void MurmurHash3_x64_128 ( const void * key, const int len,
							   const uint32_t seed, void * out )
	{
	  const uint8_t * data = (const uint8_t*)key;
	  const int nblocks = len / 16;

	  uint64_t h1 = seed;
	  uint64_t h2 = seed;

	  const uint64_t c1 = BIG_CONSTANT(0x87c37b91114253d5);
	  const uint64_t c2 = BIG_CONSTANT(0x4cf5ad432745937f);

	  //----------
	  // body

	  const uint64_t * blocks = (const uint64_t *)(data);

	  for(int i = 0; i < nblocks; i++)
	  {
		uint64_t k1 = getblock64(blocks,i*2+0);
		uint64_t k2 = getblock64(blocks,i*2+1);

		k1 *= c1; k1  = ROTL64(k1,31); k1 *= c2; h1 ^= k1;

		h1 = ROTL64(h1,27); h1 += h2; h1 = h1*5+0x52dce729;

		k2 *= c2; k2  = ROTL64(k2,33); k2 *= c1; h2 ^= k2;

		h2 = ROTL64(h2,31); h2 += h1; h2 = h2*5+0x38495ab5;
	  }

	  //----------
	  // tail

	  const uint8_t * tail = (const uint8_t*)(data + nblocks*16);

	  uint64_t k1 = 0;
	  uint64_t k2 = 0;

	  switch(len & 15)
	  {
	  case 15: k2 ^= ((uint64_t)tail[14]) << 48;
	  case 14: k2 ^= ((uint64_t)tail[13]) << 40;
	  case 13: k2 ^= ((uint64_t)tail[12]) << 32;
	  case 12: k2 ^= ((uint64_t)tail[11]) << 24;
	  case 11: k2 ^= ((uint64_t)tail[10]) << 16;
	  case 10: k2 ^= ((uint64_t)tail[ 9]) << 8;
	  case  9: k2 ^= ((uint64_t)tail[ 8]) << 0;
			   k2 *= c2; k2  = ROTL64(k2,33); k2 *= c1; h2 ^= k2;

	  case  8: k1 ^= ((uint64_t)tail[ 7]) << 56;
	  case  7: k1 ^= ((uint64_t)tail[ 6]) << 48;
	  case  6: k1 ^= ((uint64_t)tail[ 5]) << 40;
	  case  5: k1 ^= ((uint64_t)tail[ 4]) << 32;
	  case  4: k1 ^= ((uint64_t)tail[ 3]) << 24;
	  case  3: k1 ^= ((uint64_t)tail[ 2]) << 16;
	  case  2: k1 ^= ((uint64_t)tail[ 1]) << 8;
	  case  1: k1 ^= ((uint64_t)tail[ 0]) << 0;
			   k1 *= c1; k1  = ROTL64(k1,31); k1 *= c2; h1 ^= k1;
	  };

	  //----------
	  // finalization

	  h1 ^= len; h2 ^= len;

	  h1 += h2;
	  h2 += h1;

	  h1 = fmix64(h1);
	  h2 = fmix64(h2);

	  h1 += h2;
	  h2 += h1;

	  ((uint64_t*)out)[0] = h1;
	  ((uint64_t*)out)[1] = h2;
	}
	 */
}

