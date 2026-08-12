# UnityTechSnippets
유니티 클라이언트 기술 모듈 및 트러블 슈팅 샘플 코드 모음

1. MentalPoker: 중앙 서버가 없는 P2P(Peer-to-Peer) 환경에서 플레이어 상호 간의 무결성을 보장하는 카드 셔플 및 암호화 알고리즘
    - Mental Poker에 암호화 기준인 교환 암호화(Commutative Encryption, $E_A(E_B(M)) = E_B(E_A(M))$) 원리를 적용
    - 3단계 카드 파이프라인 수립: 전체 덱 1차 암호화/셔플 ➔ 개별 카드 재암호화 ➔ 턴제 키 교환을 통한 복호화

2. TextPanelSplitter: 다국어 지원 시, 장문 텍스트 렌더링으로 인한 단일 UI 패널/라벨의 정점 수 제한 초과 버그 방지 모듈
    - 텍스트의 중간 지점부터 개행 문자를 탐색하여 텍스트를 2개 영역으로 동적 분할
    - 분할된 텍스트 길이에 맞춰 UI Label, Collider, Panel Clip Region 및 Local Position 자동 재배치
    - 플랫폼별(에디터/안드로이드 StreamingAssets) 파일 I/O 및 UnityWebRequest 처리 분기

3. AndroidDeepLink: 안드로이드 외부 앱 연동 딥링크 시스템
    - AndroidJavaClass 및 AndroidJavaObject를 통해 안드로이드 Intent에 FLAG_ACTIVITY_NEW_TASK 플래그 주입 (외부 앱과 게임의 Task 독립 분리)
    - URI Scheme 파싱 및 Query String Dictionary 자동 변환
    - C# event Action 기반 구독/해제 패턴을 적용하여, OnDisable/OnDestroy 시 메모리 누수(GC) 차단 구조 적용

4. UnityBuildPipeline: 안드로이드 빌드 자동화 스크립트 (Unity Editor Extensions 및 Command Line Interface(CLI/Jenkins) 환경에서 활용 가능)
    - Dev / Stage / Live 타겟 환경별 APK 및 AAB 선택적 빌드
    - 빌드 전 에셋번들 복사, Keystore 동적 서명, App Icon 및 Application Identifier 자동 전환
    - XmlDocument 기반 AndroidManifest.xml 내 URI Scheme 속성 동적 치환
    - 빌드 에러 발생 시에도 에디터 환경을 안전하게 복원하는 CleanUp 메커니즘 구축