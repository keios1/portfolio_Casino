# 🎲 [Casino] (Turn-based Dice Roguelike)

덱빌딩 전략과 주사위 기반의 전투를 결합한 턴제 로그라이크 게임입니다. 플레이어는 상황에 맞는 카드를 선택하고, 요구되는 주사위를 슬롯에 배치하여 **'스킬 + 주사위 눈금'**에 비례하는 강력한 데미지로 몬스터를 공략해야 합니다.

## 🎥 Gameplay Preview
*(아래 이미지를 클릭하시면 유튜브 플레이 영상으로 이동합니다.)*

[![[게임 이름] 플레이 영상](https://img.youtube.com/vi/여기에_유튜브_영상_ID_입력/maxresdefault.jpg)](https://www.youtube.com/watch?v=GFSS6Iol7sU))

---

## 🛠️ 핵심 구현 로직 (Key Implementations)

### 1. 확장성을 고려한 통합 몬스터 AI 시스템
* **객체 지향적 설계 (상속 & 다형성):** 모든 몬스터는 최상위 `EnemyBase`를 상속받아 공통 데미지 처리를 수행하며, 가상 코루틴을 오버라이딩하여 개별 몬스터와 보스 객체에 특화된 고유 행동 패턴(FSM)을 비동기로 구현했습니다.
* **로직 디커플링:** 몬스터 AI, 슬롯 UI 연출, 데미지 연산의 의존성을 완벽히 분리했습니다. 특히 슬롯머신은 특정 결과값을 미리 강제하지 않고, **순수하게 회전하고 정지하는 독립적인 스핀 기능**으로 구현하여 기믹의 모듈화를 달성했습니다.
* **데이터 주도 설계 (Data-Driven Design):** `ScriptableObject(SO)`를 활용해 몬스터의 스탯과 기믹 데이터를 분리하여, 프로그래머 개입 없이 기획자가 인스펙터에서 즉각적인 난이도 밸런싱을 수행할 수 있습니다.

### 2. O(1) 탐색 기반의 이벤트 주도 튜토리얼 시스템
* **개방-폐쇄 원칙 (OCP) 적용:** 튜토리얼 진행 조건과 타겟 ID 등 핵심 데이터를 `ScriptableObject`로 직렬화하여, 하드코딩 없이 데이터 확장만으로 튜토리얼을 제어할 수 있도록 유연한 아키텍처를 설계했습니다.
* **탐색 최적화 (Self-Registration Pattern):** 런타임 환경에서 무거운 `GameObject.Find()` 연산을 배제하기 위해, 타겟 오브젝트가 활성화될 때 자신의 고유 ID를 전역 딕셔너리에 직접 매핑하고 파괴될 때 스스로 제거하도록 구현했습니다. 이를 통해 **탐색 비용을 O(1)로 최적화**하고 메모리 누수를 방지했습니다.
* **시스템 간 의존성 분리:** 튜토리얼 매니저는 상태 관리와 Action 기반의 이벤트 브로드캐스팅만 담당하며, 씬 컨트롤러가 이를 구독하여 연출을 처리하도록 설계해 스파게티 코드를 원천 차단했습니다.

---

## 💻 기술 스택 (Tech Stack)
![Unity](https://img.shields.io/badge/Unity-100000?style=flat-square&logo=unity&logoColor=white) 
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp&logoColor=white)
