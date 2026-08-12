using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public static class FindRandomPrime
{

    #region Find PrimeNumber
    public static bool IsPrime(BigInteger number)
    {
        if (number <= 1) return false;
        if (number <= 3) return true;
        if (number % 2 == 0 || number % 3 == 0) return false;

        for (BigInteger i = 5; i * i <= number; i += 6)
        {
            if (number % i == 0 || number % (i + 2) == 0)
                return false;
        }
        return true;
    }
    public static BigInteger FindPrime()
    {
        int ranNum = 3;
        bool isPrime = false;

        while (!isPrime)
        {
            ranNum = Random.Range(3, int.MaxValue);
            isPrime = IsPrime(ranNum);
        }
        return ranNum;
    }
    #endregion
}

public class CardDataManager
{
    const int DECKNUM = 52;
    const int MAXPLAYERNUM = 1234;

    // 암호화 하는데 필요한 공용 숫자
    BigInteger Modulus;

    // 셔플할때 쓸 개인적 암호(1개)
    BigInteger CommonKey;

    // 나의 개인적 개별 암호(52개)
    BigInteger[] Keys = new BigInteger[DECKNUM];

    // 다른 사람의 개별 암호 저장
    BigInteger[,] OtherKeys = new BigInteger[MAXPLAYERNUM, DECKNUM];


    public void Initialize(BigInteger modulus)
    {
        Modulus = modulus;
        // 개인적 암호 생성 & 저장
        CommonKey = FindRandomPrime.FindPrime();
        // 개인적 개별 암호 생성 & 저장
        for(int i = 0; i < DECKNUM; i++)
        {
            Keys[i] = FindRandomPrime.FindPrime();
        }

        // OtherKeys 초기화
        for(int j = 0; j < MAXPLAYERNUM; j++)
        {
            for(int k = 0; k < DECKNUM; k++)
            {
                OtherKeys[j, k] = 0;
            }
        }
    }

    /// <summary>
    /// 전체 덱을 공통 키로 암호화하고 셔플
    /// </summary>
    public void Suffle_EncryptAll(ref BigInteger[] deck)
    {
        // encrypt 52개
        var commonEncryption = new CommutativeEncryption(CommonKey, Modulus);

        for (int i = 0; i < deck.Length; i++)
        {
            deck[i] = commonEncryption.Encrypt(deck[i]);
        }

        // suffle
        var random = new System.Random();
        deck = deck.OrderBy(x => random.Next()).ToArray();
    }

    /// <summary>
    /// 1차 셔플 암호화 해제
    /// </summary>
    public void DecryptAll(ref BigInteger[] deck)
    {
        // decrypt
        var commonEncryption = new CommutativeEncryption(CommonKey, Modulus);

        for (int i = 0; i < deck.Length; i++)
        {
            deck[i] = commonEncryption.Decrypt(deck[i]);
        }
    }

    // Phase 2

    /// <summary>
    /// 1차 암호가 풀린 각 카드에 개별 키 적용
    /// </summary>
    public void Separate_EncryptAll(ref BigInteger[] deck)
    {
        for(int i = 0; i < deck.Length; i++)
        {
            // 개별 키 encrypt
            var keyEncryption = new CommutativeEncryption(Keys[i], Modulus);
            deck[i] = keyEncryption.Encrypt(deck[i]);
        }
    }

    // Phase 3: 키 교환 및 특정 카드 복호화

    /// <summary>
    /// 특정 인덱스의 카드를 타 플레이어 키와 내 키를 조합하여 최종 복호화
    /// </summary>
    public BigInteger DecryptCard(BigInteger encryptNum, int index)
    {
        BigInteger num = encryptNum;
        for(int i = 0; i < MAXPLAYERNUM; i++)
        {
            if (OtherKeys[i, index] != 0)
            {
                var otherKeyEncryption = new CommutativeEncryption(OtherKeys[i, index], Modulus);
                num = otherKeyEncryption.Decrypt(num);
            }
        }
        var mykeyEncryption = new CommutativeEncryption(Keys[index], Modulus);
        return mykeyEncryption.Decrypt(num);
    }

    /// <summary>
    /// 타 플레이어에게 전달받은 개별 카드 복호화 키 저장
    /// </summary>
    public void RecievedOtherKeys(int[] index, BigInteger[] keys)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            OtherKeys[index[0], index[i + 1]] = keys[i];
        }
    }

    public BigInteger SendMyKey(int index)
    {
        return Keys[index];
    }

    /// <summary>
    /// (테스트용) 전체 개별 암호화 일괄 복호화
    /// </summary>
    public void Separate_DecryptAll(ref BigInteger[] deck)
    {
        for (int i = 0; i < deck.Length; i++)
        {
            var keyEncryption = new CommutativeEncryption(Keys[i], Modulus);
            deck[i] = keyEncryption.Decrypt(deck[i]);
        }
    }

}
