using System.Collections;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// 멘탈 포커 전체 플로우 테스트 코드
/// </summary>
public class CardEncryptionTest : MonoBehaviour {
    BigInteger[] Deck = new BigInteger[52];

    void Start() {
        BigInteger modulus = FindRandomPrime.FindPrime();

        var aliceCardData = new CardDataManager();
        var bobCardData = new CardDataManager();
        var charlieCardData = new CardDataManager();

        for (int i = 0; i < Deck.Length; i++) {
            Deck[i] = i + 2;
        }
        foreach (var item in Deck) {
            Debug.Log(item); // 초반 세팅값 확인
        }

        aliceCardData.Initialize(modulus);
        bobCardData.Initialize(modulus);
        charlieCardData.Initialize(modulus);
        // phase 1: 순차 셔플 & 암호화
        aliceCardData.Suffle_EncryptAll(ref Deck);
        Debug.Log("Suffle Encrypt 0 " + Deck[0]);
        bobCardData.Suffle_EncryptAll(ref Deck);
        Debug.Log("Suffle Encrypt 0 " + Deck[0]);
        charlieCardData.Suffle_EncryptAll(ref Deck);
        Debug.Log("Suffle Encrypt 0 " + Deck[0]);
        // phase 2: 플레이어 개별 키로 암호화
        aliceCardData.DecryptAll(ref Deck);
        aliceCardData.Separate_EncryptAll(ref Deck);
        bobCardData.DecryptAll(ref Deck);
        bobCardData.Separate_EncryptAll(ref Deck);
        charlieCardData.DecryptAll(ref Deck);
        charlieCardData.Separate_EncryptAll(ref Deck);
        foreach (var item in Deck) {
            Debug.Log(item); // 최종 암호화 된 값 확인
        }
        // 모든 암호 풀기
        aliceCardData.Separate_DecryptAll(ref Deck);
        charlieCardData.Separate_DecryptAll(ref Deck);
        bobCardData.Separate_DecryptAll(ref Deck);
        foreach(var item in Deck) {
            Debug.Log(item); // 복호화 확인(셔플여부, 잘 복호화 되었는지 확인)
        }
    }
}
