const fs = require('fs');
const path = require('path');
const zlib = require('zlib');
const crypto = require('crypto');

function crc32(buf) {
  let table = new Uint32Array(256);
  for (let i = 0; i < 256; i++) {
    let c = i;
    for (let k = 0; k < 8; k++) c = ((c & 1) ? (0xEDB88320 ^ (c >>> 1)) : (c >>> 1));
    table[i] = c;
  }
  let crc = -1;
  for (let i = 0; i < buf.length; i++) crc = (crc >>> 8) ^ table[(crc ^ buf[i]) & 0xFF];
  return (crc ^ (-1)) >>> 0;
}

function makeChunk(type, data) {
  const len = Buffer.alloc(4);
  len.writeUInt32BE(data.length, 0);
  const t = Buffer.from(type, 'ascii');
  const crc = Buffer.alloc(4);
  crc.writeUInt32BE(crc32(Buffer.concat([t, data])), 0);
  return Buffer.concat([len, t, data, crc]);
}

function createPng(width, height, rgbaBuffer) {
  const header = Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]);
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(width, 0);
  ihdr.writeUInt32BE(height, 4);
  ihdr[8] = 8;
  ihdr[9] = 6;
  ihdr[10] = 0; ihdr[11] = 0; ihdr[12] = 0;
  const ihdrChunk = makeChunk('IHDR', ihdr);

  const rawRows = [];
  for (let y = 0; y < height; y++) {
    rawRows.push(Buffer.from([0]));
    rawRows.push(rgbaBuffer.subarray(y * width * 4, (y + 1) * width * 4));
  }
  const idatChunk = makeChunk('IDAT', zlib.deflateSync(Buffer.concat(rawRows)));
  const iendChunk = makeChunk('IEND', Buffer.alloc(0));

  return Buffer.concat([header, ihdrChunk, idatChunk, iendChunk]);
}

function makeCanvas(w = 32, h = 32) {
  const buf = Buffer.alloc(w * h * 4, 0);
  return {
    w, h, buf,
    set(x, y, r, g, b, a = 255) {
      if (x < 0 || x >= w || y < 0 || y >= h) return;
      const idx = (y * w + x) * 4;
      buf[idx] = r;
      buf[idx + 1] = g;
      buf[idx + 2] = b;
      buf[idx + 3] = a;
    },
    fillRect(x1, y1, x2, y2, r, g, b, a = 255) {
      const minX = Math.max(0, Math.min(x1, x2));
      const maxX = Math.min(w - 1, Math.max(x1, x2));
      const minY = Math.max(0, Math.min(y1, y2));
      const maxY = Math.min(h - 1, Math.max(y1, y2));
      for (let y = minY; y <= maxY; y++) {
        for (let x = minX; x <= maxX; x++) {
          this.set(x, y, r, g, b, a);
        }
      }
    },
    fillCircle(cx, cy, radius, r, g, b, a = 255) {
      for (let y = -radius; y <= radius; y++) {
        for (let x = -radius; x <= radius; x++) {
          if (x * x + y * y <= radius * radius) {
            this.set(Math.round(cx + x), Math.round(cy + y), r, g, b, a);
          }
        }
      }
    },
    strokeRect(x1, y1, x2, y2, r, g, b, a = 255) {
      this.fillRect(x1, y1, x2, y1, r, g, b, a);
      this.fillRect(x1, y2, x2, y2, r, g, b, a);
      this.fillRect(x1, y1, x1, y2, r, g, b, a);
      this.fillRect(x2, y1, x2, y2, r, g, b, a);
    }
  };
}

function saveSprite(relPath, canvas) {
  const pngData = createPng(canvas.w, canvas.h, canvas.buf);
  const targets = [
    path.join(__dirname, '..', relPath),
    path.join(__dirname, '..', relPath.replace('Assets/Sprites/Icons', 'Assets/Resources/SkillIcons'))
  ];

  for (const fullPath of targets) {
    fs.mkdirSync(path.dirname(fullPath), { recursive: true });
    fs.writeFileSync(fullPath, pngData);

    const metaPath = fullPath + '.meta';
    if (!fs.existsSync(metaPath)) {
      const guid = crypto.randomBytes(16).toString('hex');
      const meta = `fileFormatVersion: 2
guid: ${guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
  isReadable: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    wrapU: 1
    wrapV: 1
    wrapW: 1
  spriteMode: 1
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 32
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
`;
      fs.writeFileSync(metaPath, meta);
    }
  }
  console.log('Saved skill icon:', path.basename(relPath));
}

// ==========================================
// 1. 일반 공격 무기 9종 (Weapons)
// ==========================================

// 1. CleanBullet (클린 탄환: 푸른 물방울 / 맑은 세정 탄환)
{
  const c = makeCanvas();
  c.fillCircle(16, 27, 8, 0, 0, 0, 60); // 바닥 그림자
  // 물방울 몸체
  c.fillCircle(16, 18, 9, 30, 140, 240); // 딥 블루
  c.fillCircle(16, 17, 7, 70, 190, 255); // 밝은 청록
  c.fillCircle(14, 14, 4, 180, 235, 255); // 하이라이트 코어
  // 뾰족한 위쪽 꼭지
  c.fillRect(15, 6, 17, 10, 30, 140, 240);
  c.fillRect(16, 4, 16, 7, 70, 190, 255);
  // 광택 점
  c.set(13, 13, 255, 255, 255);
  c.set(14, 12, 255, 255, 255);
  saveSprite('Assets/Sprites/Icons/icon_clean_bullet.png', c);
}

// 2. OrbitBarrier (가디언 위성: 궤도 회전 방어 실드 위성)
{
  const c = makeCanvas();
  c.fillCircle(16, 16, 13, 0, 0, 0, 40); // 궤도 링 그림자
  // 궤도 타원 링
  for (let a = 0; a < 360; a += 10) {
    const rad = a * Math.PI / 180;
    const x = Math.round(16 + 12 * Math.cos(rad));
    const y = Math.round(16 + 5 * Math.sin(rad));
    c.set(x, y, 100, 200, 255, 180);
  }
  // 위성 구체 코어
  c.fillCircle(16, 16, 7, 240, 180, 30); // 골드
  c.fillCircle(16, 16, 5, 255, 225, 80);
  c.fillRect(15, 14, 17, 18, 255, 255, 255);
  // 솔라 패널 (좌우 날개)
  c.fillRect(6, 14, 9, 18, 50, 130, 220);
  c.strokeRect(6, 14, 9, 18, 30, 70, 140);
  c.fillRect(23, 14, 26, 18, 50, 130, 220);
  c.strokeRect(23, 14, 26, 18, 30, 70, 140);
  saveSprite('Assets/Sprites/Icons/icon_orbit_barrier.png', c);
}

// 3. LaserBeam (디톡스 레이저: 고출력 관통 레이저 건)
{
  const c = makeCanvas();
  // 레이저 포신 (사선 방향)
  c.fillRect(6, 20, 14, 26, 60, 65, 80);
  c.fillRect(8, 16, 16, 22, 100, 110, 135);
  c.fillRect(13, 13, 18, 18, 200, 70, 240); // 보라빛 파워 코어
  // 방출되는 강력한 레이저 빔
  c.fillRect(17, 10, 28, 14, 255, 60, 180); // 외곽 마젠타
  c.fillRect(18, 11, 28, 13, 255, 220, 255); // 중심 화이트 빔
  // 빔 발광 스파크
  c.set(29, 8, 255, 200, 255);
  c.set(29, 16, 255, 200, 255);
  c.set(16, 8, 240, 100, 255);
  saveSprite('Assets/Sprites/Icons/icon_laser_beam.png', c);
}

// 4. FirewallMine (방화벽 지뢰: 네온 오렌지 방화벽 지뢰)
{
  const c = makeCanvas();
  c.fillCircle(16, 27, 8, 0, 0, 0, 60);
  // 지뢰 본체 8각 금속 바디
  c.fillCircle(16, 16, 10, 45, 45, 55);
  c.fillCircle(16, 16, 8, 80, 85, 95);
  // 경고 오렌지 링
  c.fillCircle(16, 16, 6, 255, 120, 20);
  c.fillCircle(16, 16, 4, 255, 220, 40);
  // 센서 십자 LED
  c.fillRect(15, 10, 17, 22, 255, 60, 20);
  c.fillRect(10, 15, 22, 17, 255, 60, 20);
  c.set(16, 16, 255, 255, 255);
  saveSprite('Assets/Sprites/Icons/icon_firewall_mine.png', c);
}

// 5. ShockAura (쇼크 펄스: 테슬라 방전 코일 & 번개)
{
  const c = makeCanvas();
  // 번개 볼트
  c.fillRect(15, 4, 18, 10, 255, 240, 60);
  c.fillRect(13, 9, 16, 16, 255, 240, 60);
  c.fillRect(16, 14, 20, 18, 255, 240, 60);
  c.fillRect(12, 17, 16, 24, 255, 240, 60);
  c.fillRect(14, 23, 17, 28, 255, 210, 20);
  // 푸른 고압 전기 방전 스파크
  c.set(8, 12, 100, 220, 255); c.set(24, 10, 100, 220, 255);
  c.set(9, 20, 100, 220, 255); c.set(23, 22, 100, 220, 255);
  c.set(15, 13, 255, 255, 255);
  saveSprite('Assets/Sprites/Icons/icon_shock_aura.png', c);
}

// 6. DetoxMissile (디톡스 미사일: 스마트 유도 미사일)
{
  const c = makeCanvas();
  // 대각선 탄두 미사일
  c.fillRect(18, 6, 24, 12, 230, 45, 45); // 탄두 레드
  c.fillRect(12, 12, 20, 20, 240, 240, 245); // 바디 화이트
  c.fillRect(8, 18, 14, 24, 60, 65, 80); // 후방 엔진
  // 날개
  c.fillRect(6, 15, 10, 19, 230, 45, 45);
  c.fillRect(17, 22, 21, 26, 230, 45, 45);
  // 추진 화염
  c.fillRect(4, 24, 8, 28, 255, 140, 30);
  c.fillRect(3, 26, 6, 29, 255, 230, 80);
  saveSprite('Assets/Sprites/Icons/icon_detox_missile.png', c);
}

// 7. AlgorithmVortex (알고리즘 회오리: 청록/보라 데이터 토네이도)
{
  const c = makeCanvas();
  // 소용돌이 나선 층
  c.fillCircle(16, 8, 10, 70, 230, 210); // 상단 거대 회오리 폭
  c.fillCircle(16, 8, 7, 30, 160, 150);
  c.fillCircle(16, 15, 7, 80, 200, 255);
  c.fillCircle(16, 21, 5, 140, 100, 240);
  c.fillCircle(16, 26, 3, 200, 80, 255); // 회오리 꼭지
  // 회전하는 데이터 큐브 조각들
  c.fillRect(7, 6, 9, 8, 255, 255, 255);
  c.fillRect(22, 10, 24, 12, 255, 255, 255);
  c.fillRect(11, 18, 13, 20, 255, 255, 120);
  saveSprite('Assets/Sprites/Icons/icon_algorithm_vortex.png', c);
}

// 8. SanitizerMist (멸균 소독 안개: 민트 그린 스프레이)
{
  const c = makeCanvas();
  // 스프레이 보틀 헤드
  c.fillRect(6, 16, 14, 27, 40, 140, 110);
  c.fillRect(8, 10, 12, 16, 200, 210, 220);
  c.fillRect(10, 8, 14, 11, 230, 235, 245);
  // 노즐
  c.fillRect(13, 9, 16, 12, 80, 90, 100);
  // 뿜어져 나오는 미스트 안개 구름
  c.fillCircle(22, 10, 5, 80, 240, 170);
  c.fillCircle(25, 16, 6, 50, 210, 150);
  c.fillCircle(20, 19, 4, 110, 255, 200);
  c.set(24, 8, 255, 255, 255);
  c.set(27, 15, 255, 255, 255);
  saveSprite('Assets/Sprites/Icons/icon_sanitizer_mist.png', c);
}

// 9. CyberScythe (사이버 낫: 보랏빛 네온 에너지를 두른 대형 낫)
{
  const c = makeCanvas();
  // 긴 손잡이 (사선)
  for (let i = 0; i < 20; i++) {
    c.set(8 + i, 26 - i, 80, 80, 95);
    c.set(8 + i + 1, 26 - i, 50, 50, 65);
  }
  // 거대한 낫 칼날 (곡선)
  c.fillRect(18, 6, 26, 9, 180, 60, 255);
  c.fillRect(23, 7, 28, 13, 210, 100, 255);
  c.fillRect(25, 11, 29, 18, 160, 40, 240);
  c.fillRect(26, 16, 28, 22, 230, 150, 255);
  // 칼날 예리한 빛
  c.set(27, 8, 255, 255, 255);
  c.set(28, 14, 255, 255, 255);
  saveSprite('Assets/Sprites/Icons/icon_cyber_scythe.png', c);
}

// ==========================================
// 2. 패시브 보조 능력 6종 (Passives)
// ==========================================

// 10. AttackSpeed (에스프레소: 피어오르는 커피잔)
{
  const c = makeCanvas();
  c.fillCircle(16, 27, 8, 0, 0, 0, 50);
  // 머그잔 받침
  c.fillRect(9, 24, 23, 26, 220, 225, 235);
  // 머그잔 바디
  c.fillRect(10, 14, 22, 24, 235, 240, 248);
  c.fillRect(11, 14, 21, 17, 90, 50, 30); // 커피 크레마
  // 손잡이
  c.fillRect(22, 16, 25, 22, 220, 225, 235);
  c.fillRect(22, 18, 23, 20, 0, 0, 0, 0);
  // 번개 모양 피어오르는 뜨거운 김
  c.set(13, 10, 255, 200, 80);
  c.set(14, 8, 255, 200, 80);
  c.set(13, 6, 255, 220, 120);
  c.set(18, 10, 255, 200, 80);
  c.set(17, 8, 255, 200, 80);
  c.set(18, 6, 255, 220, 120);
  saveSprite('Assets/Sprites/Icons/icon_attack_speed.png', c);
}

// 11. MaxHP (하트 비타민: 빛나는 레드/핑크 하트 알약)
{
  const c = makeCanvas();
  c.fillCircle(16, 27, 8, 0, 0, 0, 50);
  // 하트 모양 바디
  c.fillCircle(12, 13, 6, 245, 50, 85);
  c.fillCircle(20, 13, 6, 245, 50, 85);
  c.fillRect(8, 13, 24, 18, 245, 50, 85);
  // 하트 꼭지 역삼각형
  for (let i = 0; i < 8; i++) {
    c.fillRect(8 + i, 18 + i, 24 - i, 19 + i, 245, 50, 85);
  }
  // 하이라이트 & 십자 마크
  c.set(10, 11, 255, 255, 255);
  c.set(11, 10, 255, 255, 255);
  c.fillRect(15, 14, 17, 20, 255, 255, 255);
  c.fillRect(13, 16, 19, 18, 255, 255, 255);
  saveSprite('Assets/Sprites/Icons/icon_max_hp.png', c);
}

// 12. DamageBoost (집중력 앰플: 붉은 에너지 앰플)
{
  const c = makeCanvas();
  c.fillCircle(16, 27, 7, 0, 0, 0, 50);
  // 앰플 실린더 유리병
  c.fillRect(11, 12, 21, 24, 220, 230, 245);
  c.fillRect(12, 13, 20, 23, 235, 40, 55); // 붉은 농축액
  // 앰플 상단 마개
  c.fillRect(13, 7, 19, 11, 180, 190, 205);
  c.fillRect(14, 5, 18, 7, 140, 145, 160);
  // 내부 도파민 에너지 버블
  c.set(14, 15, 255, 180, 190);
  c.set(17, 19, 255, 180, 190);
  c.set(13, 14, 255, 255, 255);
  saveSprite('Assets/Sprites/Icons/icon_damage_boost.png', c);
}

// 13. MoveSpeed (달콤한 신발: 날개 달린 고속 러닝화)
{
  const c = makeCanvas();
  c.fillCircle(16, 27, 9, 0, 0, 0, 50);
  // 신발 몸체 (스포티한 사이안 & 화이트)
  c.fillRect(8, 17, 22, 23, 0, 180, 230);
  c.fillRect(6, 21, 24, 25, 240, 245, 250); // 밑창
  c.fillRect(6, 25, 25, 26, 40, 45, 55); // 고무창
  // 날개 장식 (신발 뒤꿈치)
  c.fillRect(7, 12, 14, 14, 255, 255, 255);
  c.fillRect(5, 9, 12, 11, 255, 255, 255);
  c.fillRect(4, 7, 9, 9, 255, 220, 80);
  // 스피드 라인
  c.set(26, 22, 0, 220, 255);
  c.set(28, 23, 0, 220, 255);
  saveSprite('Assets/Sprites/Icons/icon_move_speed.png', c);
}

// 14. Magnet (도파민 자석: 빨강/파랑 말굽 자석)
{
  const c = makeCanvas();
  c.fillCircle(16, 27, 8, 0, 0, 0, 50);
  // U자 말굽 자석 베이스
  c.fillRect(8, 9, 13, 22, 230, 40, 50); // N극 레드
  c.fillRect(19, 9, 24, 22, 40, 110, 230); // S극 블루
  c.fillRect(8, 19, 24, 24, 180, 180, 190); // 하단 은색 연결부
  // 극 팁 (실버)
  c.fillRect(8, 7, 13, 9, 235, 240, 245);
  c.fillRect(19, 7, 24, 9, 235, 240, 245);
  // 자기장 효과 파동
  c.set(6, 6, 255, 215, 60);
  c.set(25, 6, 255, 215, 60);
  c.set(16, 5, 255, 215, 60);
  saveSprite('Assets/Sprites/Icons/icon_magnet.png', c);
}

// 15. CritChance (통찰의 안경: 스마트 크리티컬 렌즈)
{
  const c = makeCanvas();
  // 안경테 (골드)
  c.fillRect(7, 12, 25, 14, 245, 190, 40);
  // 좌/우 렌즈
  c.fillRect(7, 14, 14, 21, 40, 210, 255);
  c.strokeRect(6, 13, 15, 22, 245, 190, 40);
  c.fillRect(17, 14, 24, 21, 40, 210, 255);
  c.strokeRect(16, 13, 25, 22, 245, 190, 40);
  // 크리티컬 조준선 타겟 마크
  c.set(10, 17, 255, 50, 50);
  c.set(21, 17, 255, 50, 50);
  c.set(8, 15, 255, 255, 255);
  c.set(18, 15, 255, 255, 255);
  saveSprite('Assets/Sprites/Icons/icon_crit_chance.png', c);
}

// ==========================================
// 3. 초월 진화 무기 9종 (Evolutions - 화려한 골드 & 빛 이펙트)
// ==========================================

// 16. EvoGigaGatling (기가 개틀링: 황금 다총열 개틀링)
{
  const c = makeCanvas();
  c.fillRect(6, 11, 18, 22, 240, 180, 40); // 황금 리시버
  c.strokeRect(6, 11, 18, 22, 160, 110, 20);
  // 4연장 총열
  c.fillRect(18, 10, 27, 12, 255, 215, 70);
  c.fillRect(18, 14, 28, 16, 255, 235, 120);
  c.fillRect(18, 18, 28, 20, 255, 235, 120);
  c.fillRect(18, 22, 27, 24, 255, 215, 70);
  // 머즐 플래시 폭발 화염
  c.set(29, 13, 255, 100, 30);
  c.set(30, 17, 255, 255, 200);
  c.set(29, 21, 255, 100, 30);
  saveSprite('Assets/Sprites/Icons/icon_evo_giga_gatling.png', c);
}

// 17. EvoGalacticCleanse (은하계 정화 결계: 황금 오비탈 갤럭시 쉴드)
{
  const c = makeCanvas();
  c.fillCircle(16, 16, 13, 255, 215, 50, 80);
  // 중앙 거대 황금 코어 행성
  c.fillCircle(16, 16, 7, 255, 200, 40);
  c.fillCircle(16, 16, 5, 255, 250, 150);
  // 4방향 황금 위성 노드
  c.fillCircle(16, 4, 3, 255, 240, 80);
  c.fillCircle(16, 28, 3, 255, 240, 80);
  c.fillCircle(4, 16, 3, 255, 240, 80);
  c.fillCircle(28, 16, 3, 255, 240, 80);
  saveSprite('Assets/Sprites/Icons/icon_evo_galactic_cleanse.png', c);
}

// 18. EvoPrismCannon (프리즘 정화포: 4색 무지개 프리즘 캐논)
{
  const c = makeCanvas();
  // 다이아몬드 프리즘 크리스탈
  c.fillCircle(16, 16, 8, 230, 245, 255);
  c.fillRect(15, 8, 17, 24, 255, 255, 255);
  c.fillRect(8, 15, 24, 17, 255, 255, 255);
  // 4방향 방출 무지개 빔
  c.fillRect(15, 2, 17, 8, 255, 60, 60); // 상: 레드
  c.fillRect(15, 24, 17, 30, 60, 240, 60); // 하: 그린
  c.fillRect(2, 15, 8, 17, 60, 160, 255); // 좌: 블루
  c.fillRect(24, 15, 30, 17, 255, 220, 60); // 우: 옐로
  saveSprite('Assets/Sprites/Icons/icon_evo_prism_cannon.png', c);
}

// 19. EvoSecurityMatrix (절대 보안 결계: 큐브 홀로그램 매트릭스)
{
  const c = makeCanvas();
  // 3D 큐브 와이어프레임
  c.strokeRect(9, 9, 23, 23, 0, 240, 255);
  c.strokeRect(12, 6, 26, 20, 0, 180, 230);
  c.set(9, 9, 255, 255, 255);
  c.set(23, 9, 255, 255, 255);
  c.set(9, 23, 255, 255, 255);
  c.set(23, 23, 255, 255, 255);
  // 중앙 락 코어
  c.fillCircle(16, 16, 4, 255, 120, 40);
  c.set(16, 16, 255, 255, 255);
  saveSprite('Assets/Sprites/Icons/icon_evo_security_matrix.png', c);
}

// 20. EvoTeslaDischarge (테슬라 뇌파 방전: 거대 플라즈마 구체)
{
  const c = makeCanvas();
  c.fillCircle(16, 16, 9, 130, 50, 240);
  c.fillCircle(16, 16, 6, 210, 140, 255);
  c.fillCircle(16, 16, 3, 255, 255, 255);
  // 사방으로 뻗는 연쇄 번개 가지
  c.set(8, 7, 240, 200, 255); c.set(10, 10, 255, 255, 255);
  c.set(24, 7, 240, 200, 255); c.set(22, 10, 255, 255, 255);
  c.set(6, 22, 240, 200, 255); c.set(26, 23, 240, 200, 255);
  saveSprite('Assets/Sprites/Icons/icon_evo_tesla_discharge.png', c);
}

// 21. EvoAegisBarrage (궤도 폭격 이지스: 위성 요새 미사일)
{
  const c = makeCanvas();
  // 궤도 요새 기지
  c.fillRect(8, 14, 24, 20, 50, 60, 80);
  c.strokeRect(8, 14, 24, 20, 255, 200, 50);
  // 요새에서 일제 발사되는 미사일들
  c.fillRect(10, 6, 12, 13, 240, 50, 50);
  c.fillRect(15, 4, 17, 13, 240, 50, 50);
  c.fillRect(20, 6, 22, 13, 240, 50, 50);
  // 발사 섬광
  c.fillRect(9, 12, 23, 14, 255, 220, 80);
  saveSprite('Assets/Sprites/Icons/icon_evo_aegis_barrage.png', c);
}

// 22. EvoDetoxTyphoon (하이퍼 디톡스 타이푼: 황금 초거대 사이클론)
{
  const c = makeCanvas();
  c.fillCircle(16, 10, 11, 255, 215, 40);
  c.fillCircle(16, 10, 7, 220, 160, 20);
  c.fillCircle(16, 18, 8, 255, 235, 100);
  c.fillCircle(16, 24, 5, 255, 250, 180);
  // 태풍의 눈
  c.fillCircle(16, 10, 3, 255, 255, 255);
  saveSprite('Assets/Sprites/Icons/icon_evo_detox_typhoon.png', c);
}

// 23. EvoPlasmaCloud (절대 멸균 플라즈마: 청록 바이오 플라즈마 코어)
{
  const c = makeCanvas();
  c.fillCircle(16, 16, 11, 40, 220, 180, 150);
  c.fillCircle(16, 16, 8, 70, 255, 210);
  c.fillCircle(16, 16, 4, 230, 255, 245);
  // 외부 유독물질 정화 오라 파티클
  c.set(6, 9, 140, 255, 220); c.set(25, 8, 140, 255, 220);
  c.set(7, 24, 140, 255, 220); c.set(24, 23, 140, 255, 220);
  saveSprite('Assets/Sprites/Icons/icon_evo_plasma_cloud.png', c);
}

// 24. EvoShutdownBlade (절대 셧다운 엑스칼리버: 거대 골드 대검)
{
  const c = makeCanvas();
  // 대각선 엑스칼리버 황금 대검
  for (let i = 0; i < 22; i++) {
    c.fillRect(6 + i, 26 - i, 8 + i, 27 - i, 255, 220, 60);
  }
  // 칼날 중심 빛
  for (let i = 3; i < 20; i++) {
    c.set(7 + i, 26 - i, 255, 255, 255);
  }
  // 칼자루 크로스가드
  c.fillRect(6, 21, 12, 27, 200, 140, 30);
  c.set(26, 5, 255, 255, 255); // 검 끝 반짝임
  saveSprite('Assets/Sprites/Icons/icon_evo_shutdown_blade.png', c);
}

// ==========================================
// 4. 만렙 보너스 포상 3종 (Bonuses)
// ==========================================

// 25. BonusGold (골드 성과급: 황금빛 가득한 코인 주머니)
{
  const c = makeCanvas();
  c.fillCircle(16, 27, 8, 0, 0, 0, 50);
  // 복주머니 형태 황금 자루
  c.fillCircle(16, 19, 9, 235, 175, 30);
  c.fillRect(11, 10, 21, 14, 200, 140, 20); // 묶음 입구
  c.fillRect(13, 8, 19, 10, 245, 190, 45); // 리본 깃
  // 붉은 끈
  c.fillRect(11, 12, 21, 13, 220, 40, 40);
  // 황금 코인 마크 ($)
  c.set(16, 17, 255, 255, 255);
  c.set(16, 21, 255, 255, 255);
  c.fillRect(15, 18, 18, 20, 255, 240, 100);
  saveSprite('Assets/Sprites/Icons/icon_bonus_gold.png', c);
}

// 26. BonusHeal (풀 리커버리: 구급 키트 + 힐링 케이크)
{
  const c = makeCanvas();
  c.fillCircle(16, 27, 8, 0, 0, 0, 50);
  // 구급상자 바디
  c.fillRect(8, 11, 24, 24, 245, 245, 250);
  c.strokeRect(8, 11, 24, 24, 210, 215, 225);
  // 그린 메딕 십자가
  c.fillRect(14, 13, 18, 22, 40, 200, 110);
  c.fillRect(11, 16, 21, 19, 40, 200, 110);
  // 손잡이
  c.strokeRect(13, 7, 19, 11, 150, 155, 165);
  saveSprite('Assets/Sprites/Icons/icon_bonus_heal.png', c);
}

// 27. BonusBomb (전체 초기화: 시한 네온 폭탄)
{
  const c = makeCanvas();
  c.fillCircle(16, 27, 8, 0, 0, 0, 50);
  // 폭탄 둥근 몸체 (다크 슬레이트)
  c.fillCircle(16, 18, 9, 35, 35, 45);
  c.fillCircle(16, 18, 7, 50, 55, 70);
  // 도화선 & 불꽃
  c.fillRect(14, 8, 18, 11, 180, 140, 90);
  c.set(16, 6, 255, 140, 30);
  c.set(17, 5, 255, 230, 60);
  c.set(18, 4, 255, 60, 40);
  // 해골 위협 표시
  c.set(14, 16, 255, 255, 255); c.set(18, 16, 255, 255, 255);
  c.fillRect(15, 19, 17, 20, 255, 255, 255);
  saveSprite('Assets/Sprites/Icons/icon_bonus_bomb.png', c);
}

console.log('🎉 Successfully generated all 27 pixel art dot skill icons!');
