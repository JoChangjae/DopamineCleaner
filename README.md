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

1. **자동 조준 & 무기 진화 (Weapon Evolutions)**
   * **클린 탄환 (Clean Bullet)** $\rightarrow$ **기가 개틀링 (Giga Gatling)**: 8방향 고속 연사 탄막
   * **가디언 위성 (Orbit Barrier)** $\rightarrow$ **은하계 정화 결계 (Galactic Cleanse)**: 6개의 황금 위성 넉백 배리어
2. **뱀서식 스마트 경험치 보석 압축 (Smart Gem Merging)**
   * 필드에 드랍되는 경험치 보석이 45개 이상이거나 주변 보석과 인접할 경우, 하나의 고가치 보석으로 자동 누적 병합(초록 $\rightarrow$ 파랑 $\rightarrow$ 핑크 $\rightarrow$ 거대 핑크)되어 프레임 드랍 없이 부드러운 60 FPS 유지.
3. **스테이지 & 보스 돌진 시스템**
   * 일반 몬스터를 목표치만큼 처치하면 스테이지 거대 보스가 출현합니다.
   * 보스 격파 시 다음 스테이지로 진입하며 100% 황금 보물상자와 전리품이 쏟아집니다.
4. **특수 필드 아이템**
   * 🍰 **멘탈 케이크**: 체력 30 즉시 회복
   * 💣 **디톡스 폭탄**: 화면의 모든 일반 잡생각 몬스터 일망타진
   * 🧲 **도파민 자석**: 필드의 모든 경험치 보석을 즉시 흡수
   * 🎁 **황금 보물상자**: 스킬 무료 강화 및 무기 진화

---

## 📁 프로젝트 구조 (Assets/Scripts)

```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs       # 게임 상태(Ready/Playing/LevelUp/GameOver), 스테이지, 킬 카운트
│   └── CameraFollow.cs      # 2D 부드러운 플레이어 추적 카메라 (LateUpdate SmoothDamp)
├── Characters/
│   └── PlayerController.cs  # 2D 물리 이동, 대시, 체력, 무적, 경험치 & 레벨업, 부활
├── Enemies/
│   ├── Enemy.cs             # 잡생각 먼지, 쏟아지는 졸음, 알림 유령, 도파민 보스 AI
│   └── EnemySpawner.cs      # Mob Density Cap(90마리 제한), 웨이브 러시, 보스 스폰
├── Weapons/
│   ├── WeaponBase.cs        # 무기 기본 추상 클래스
│   ├── CleanBulletWeapon.cs # 자동 사격 및 기가 개틀링 진화
│   ├── OrbitWeapon.cs       # 가디언 궤도 위성 회전 무기
│   ├── Projectile.cs        # 관통 탄환 물리 및 대미지
│   └── WeaponManager.cs     # 레벨업 3선택 카드 생성, 패시브 강화 및 진화 판정
├── Items/
│   ├── ExpGem.cs            # 경험치 보석 및 특수 아이템 (자석 흡수, 자동 병합)
│   └── ItemManager.cs       # 보석 풀링, 전역 폭탄/자석 발동
└── UI/
    ├── UIManager.cs         # 상단 HUD (체력, 경험치, 타이머, 대시 쿨타임) 및 모달 제어
    └── UpgradeCardUI.cs     # 레벨업 카드 선택 UI
```
