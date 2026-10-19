# 🧠 도파민 클리너 (Dopamine Cleaner)

현대인의 뇌를 잠식하는 잡생각, 숏폼 알고리즘, 졸음 몬스터를 시원하게 정화하는 **2D 탑다운 뱀서라이크(Survivors-like) 로그라이트 액션 게임**입니다.

---

## 🎮 게임 정보

* **엔진**: Unity 6 (6000.6.5f1)
* **장르**: 2D Roguelite, Survivors-like, Bullet Hell
* **개발 언어**: C# (.NET Standard)
* **타겟 플랫폼**: PC (Steam), 모바일 (Android/iOS)

---

## 🕹️ 조작법 (Controls)

| 키 | 동작 |
| :--- | :--- |
| **W, A, S, D** / 방향키 | 2D 8방향 이동 |
| **Space** / **Left Shift** | 무적 회피 대시 (Dash) |
| **P** / **ESC** | 일시정지 (Pause) |
| **마우스 클릭** | 레벨업 카드 선택 & UI 상호작용 |

---

## ⚔️ 핵심 시스템 및 특징

1. **자동 조준 & 5대 무기 진화 (Weapon Evolutions)**
   * **클린 탄환 (Clean Bullet)** $\rightarrow$ **기가 개틀링 (Giga Gatling)**: 8방향 고속 연사 탄막
   * **가디언 위성 (Orbit Barrier)** $\rightarrow$ **은하계 정화 결계 (Galactic Cleanse)**: 6개의 황금 위성 넉백 배리어
   * **디톡스 레이저 (Laser Beam)** $\rightarrow$ **프리즘 정화포 (Prism Cannon)**: 십자 4방향 고출력 관통 레이저
   * **방화벽 지뢰 (Firewall Mine)** $\rightarrow$ **절대 보안 결계 (Security Matrix)**: 초대형 폭발 트랩 투하
   * **쇼크 펄스 (Shock Aura)** $\rightarrow$ **테슬라 뇌파 방전 (Tesla Discharge)**: 화면 전체 연쇄 벼락 폭풍
2. **🔥 초집중 디톡스 피버 (Fever Mode) & 콤보 시스템**
   * 몬스터 연속 처치 시 콤보 누적 및 디톡스 피버 게이지 충전!
   * 100% 도달 시 **피버 모드 발동**: 공격속도 1.8배 폭주, 이동속도 +30%, 피격 대미지 60% 경감, 화면 내 모든 보석 자동 흡수!
3. **💥 박진감 넘치는 타격 연출 & Zero-Asset 절차적 SFX 사운드**
   * **플로팅 대미지 텍스트**: 일반/크리티컬 수치 팝업
   * **카메라 쉐이크 & 피격 플래시**: 폭발 및 타격 시 역동적인 연출
   * **내장 사운드 엔진**: 외부 에셋 없이 런타임 합성 오디오로 발사음, 타격음, 보석음, 레벨업 팡파르 출력
4. **📊 도파민 디톡스 종합 검진표 & 무기별 DPS 차트**
   * 게임 오버/생존 성공 시 "정신 건강 검진표" 스타일의 결과창 출력.
   * 종합 등급 평가 (S/A/B/C) 및 숏폼 절약 편수, 소독한 잡생각 수, 획득 재화 정산.
   * 각 무기별 총 누적 피해량 및 비율(%) 시각화 바 차트 제공.
5. **⚠️ 작전 중 돌발 긴급 퀘스트 (Emergency Quests)**
   * 생존 중 주기적으로 긴급 작전 지령 발령 (잡생각 급속 소독, 숏폼 러너 집중 격퇴 등).
   * 제한 시간 내 성공 시 **즉시 황금 보물상자 투하 + 초집중 디톡스 피버 모드 발동**!
6. **🌫️ 도파민 과부하 (Brain Fog) & 멘탈 쉼터 (Sanctuary Zone)**
   * 장기 생존 시 도파민 뇌 피로가 누적되어 화면 글리치 비네트 및 이동속도 둔화 발생.
   * 맵 곳곳에 생성되는 청량한 '멘탈 쉼터(디톡스 분수대)'에 들어가면 **초당 체력 회복, 과부하 정화, 이동속도 부스트** 혜택!
7. **⏸️ 인게임 일시정지 (PAUSE) & 장비/패시브 현황판**
   * `ESC` 키로 언제든 게임 일시정지 및 장착한 무기/진화 상태, 패시브 훈련 레벨, 사운드 On/Off 토글, 조기 철수(60% 정산) 가능.
8. **뱀서식 스마트 경험치 보석 압축 (Smart Gem Merging)**
   * 필드 보석이 45개 이상이거나 주변 보석과 인접할 경우 고가치 보석으로 자동 누적 병합(초록 $\rightarrow$ 파랑 $\rightarrow$ 핑크 $\rightarrow$ 거대 핑크)되어 부드러운 60 FPS 유지.
9. **특수 필드 아이템**
   * 🍰 **멘탈 케이크**: 체력 30 즉시 회복
   * 💣 **디톡스 폭탄**: 화면 내 모든 일반 몬스터 일망타진
   * 🧲 **도파민 자석**: 필드의 모든 보석 즉시 흡수
   * 🎁 **황금 보물상자**: 스킬 무료 강화 및 무기 진화

---

## 🧠 도파민 클리너 본부 시스템 (HQ & Detox Meta Loop)

1. **도파민 본부 시스템 (TownScene)**
   * **🧑‍💼 클리너 요원실**: 김인턴, 박사정, 강반장, 윤이사 4종의 고유 특성 패시브 요원 영입 및 선택.
   * **🛒 디톡스 보급소**: 응급 구급약, 비상 소생 키트, 스피드 부스터 등 실전 아이템 구매 및 인벤토리 아이템 매각.
   * **🗄️ 본부 창고**: 출동 가방 (6칸) ⇄ 본부 창고 (24칸) 아이템 보관 및 세팅.
   * **🔬 본부 연구소**: 골드 및 디톡스 포인트를 투자해 공격력, 체력, 이동속도, 자석 반경, 부활 횟수를 영구 강화하는 Perk 트리.
   * **🚀 정화 작전 출동처**: 도파민 격리 병동, 번아웃 오피스 미궁, 심야 숏폼 고속도로 맵과 4단계 오염 난이도(Easy, Normal, Hard, Nightmare) 설정 후 작전 출동.
2. **정화 액션 구역 & 귀환 (MainGame)**
   * 실시간 구급약 사용 (`1`번 키) 및 비상 소생 키트 자동 발동.
   * 숏폼 러너(지그재그 고속 돌진), 스팸 슈터(원거리 투사체), 8방향 탄막 보스 등 다채로운 적 등장.
   * 만렙 도달 후 무한 디톡스 보너스/체력/폭탄 한계 돌파 시스템.
   * 목표 생존 시간 달성 시 승리 정산 후 본부로 귀환하여 골드 및 디톡스 포인트 획득.

---

## 📁 프로젝트 구조 (Assets/Scripts)

```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs            # 게임 상태, 스테이지, 킬 카운트, 정산금 계산
│   ├── FeverManager.cs           # 🔥 디톡스 피버 모드 및 콤보 시스템
│   ├── EmergencyQuestManager.cs  # ⚠️ 작전 중 돌발 긴급 퀘스트 관리자
│   ├── MentalSanctuaryManager.cs # 🌫️ 도파민 과부하 및 멘탈 쉼터 존 관리자
│   ├── DamageTextManager.cs      # 💥 플로팅 대미지 텍스트 풀링
│   ├── SoundManager.cs           # 🎵 절차적 런타임 오디오 합성 엔진
│   └── CameraFollow.cs           # 2D 부드러운 플레이어 추적 및 카메라 쉐이크
├── Meta/
│   ├── GameEnums.cs         # 난이도, 아이템 타입/희귀도 정의
│   ├── CharacterData.cs     # 요원 스탯, 고유 특성, 해금 비용 데이터베이스
│   ├── MetaUpgradeData.cs   # 🔬 본부 영구 연구소 Perk 데이터베이스
│   ├── ItemData.cs          # 소모품, 장비, 유물 데이터베이스
│   ├── DungeonData.cs       # 정화 구역 맵, 난이도별 배율 데이터베이스
│   └── GameDataManager.cs  # 싱글톤 프로필 세이브/로드, 인벤토리/창고, 연구소 관리
├── Town/
│   ├── TownManager.cs       # 본부 UI 탭 네비게이션 및 경제 현황 표시
│   ├── CharacterSelectUI.cs # 클리너 요원 영입 및 출동 선택 UI
│   ├── ShopUI.cs            # 디톡스 보급소 구매 및 매각 UI
│   ├── StorageUI.cs         # 가방 ⇄ 본부 창고 UI
│   ├── ResearchUI.cs        # 🔬 본부 연구소 Perk 강화 UI
│   └── DungeonSelectUI.cs   # 정화 구역 맵 및 난이도 선택, 작전 브리핑 UI
├── Characters/
│   └── PlayerController.cs  # 2D 물리 이동, 대시, 체력, 무적, 경험치 & 레벨업, 구급약 사용
├── Enemies/
│   ├── Enemy.cs             # 몬스터 AI (돌진, 숏폼 러너, 원거리 스팸, 보스 탄막)
│   ├── EnemyProjectile.cs   # 원거리 몬스터 및 보스 탄막 투사체
│   └── EnemySpawner.cs      # 몹 스폰 풀링, 웨이브 러시, 보스 스폰
├── Weapons/
│   ├── WeaponBase.cs        # 무기 기본 추상 클래스
│   ├── CleanBulletWeapon.cs # 자동 사격 및 기가 개틀링 진화
│   ├── OrbitWeapon.cs       # 가디언 궤도 위성 회전 무기 및 은하계 정화 진화
│   ├── LaserBeamWeapon.cs   # ⚡ 디톡스 레이저 및 프리즘 정화포 진화
│   ├── FirewallMineWeapon.cs# 💣 방화벽 지뢰 및 절대 보안 결계 진화
│   ├── ShockAuraWeapon.cs   # ⚡ 쇼크 펄스 및 테슬라 뇌파 방전 진화
│   ├── OrbitSatelliteContact.cs # 위성 회전 충돌체
│   ├── Projectile.cs        # 관통 탄환 물리 및 대미지
│   └── WeaponManager.cs     # 5대 무기 레벨업 카드, 만렙 한계돌파, 진화 판정
├── Items/
│   ├── ExpGem.cs            # 경험치 보석 및 특수 아이템 (자석 흡수, 자동 병합)
│   └── ItemManager.cs       # 보석 풀링, 전역 폭탄/자석 발동
├── UI/
│   ├── UIManager.cs         # 인게임 HUD(피버바, 콤보, 체력, EXP), 레벨업/정산 모달
│   └── UpgradeCardUI.cs     # 레벨업 카드 UI
└── Editor/
    ├── TownSceneBuilder.cs  # 도파민 본부 씬(TownScene) 자동 생성 빌더
    └── DopamineSceneBuilder.cs # 액션 구역 씬(MainGame) 및 원클릭 빌드 도구
```
