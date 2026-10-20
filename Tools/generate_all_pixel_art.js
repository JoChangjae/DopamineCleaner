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

function makeCanvas(w, h, bgR = 0, bgG = 0, bgB = 0, bgA = 0) {
  const buf = Buffer.alloc(w * h * 4, 0);
  if (bgA > 0) {
    for (let i = 0; i < w * h; i++) {
      buf[i * 4] = bgR;
      buf[i * 4 + 1] = bgG;
      buf[i * 4 + 2] = bgB;
      buf[i * 4 + 3] = bgA;
    }
  }
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

function writeSprite(relPath, canvas, ppu = 32) {
  const fullPath = path.join(__dirname, '..', relPath);
  fs.mkdirSync(path.dirname(fullPath), { recursive: true });
  fs.writeFileSync(fullPath, createPng(canvas.w, canvas.h, canvas.buf));

  if (!fs.existsSync(fullPath + '.meta')) {
    const guid = crypto.randomBytes(16).toString('hex');
    const metaContent = `fileFormatVersion: 2
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
  spritePixelsToUnits: ${ppu}
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
`;
    fs.writeFileSync(fullPath + '.meta', metaContent);
  }
  console.log('Saved pixel art:', relPath);
}

// ==============================================================
// 1. 4종 플레이어 요원 도트 캐릭터 (32x32)
// ==============================================================

// (A) char_intern (김인턴 - 깔끔한 네이비 정장 + 빗자루 + 노란 명찰)
function generateCharIntern() {
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 28, 9, 0, 0, 0, 70); // 그림자
  // 머리카락 (단정한 짙은 갈색)
  c.fillRect(11, 4, 21, 9, 65, 40, 25);
  c.fillRect(12, 3, 20, 5, 80, 50, 35);
  // 얼굴 (살구빛 피부)
  c.fillRect(12, 8, 20, 14, 255, 215, 180);
  // 반짝이는 눈
  c.set(14, 10, 40, 40, 50); c.set(18, 10, 40, 40, 50);
  c.set(14, 9, 255, 255, 255); c.set(18, 9, 255, 255, 255);
  // 발그레한 뺨
  c.set(13, 12, 255, 150, 150); c.set(19, 12, 255, 150, 150);
  // 미소
  c.fillRect(15, 12, 17, 12, 220, 80, 80);
  // 셔츠 & 타이 (화이트 셔츠 & 붉은 넥타이)
  c.fillRect(11, 15, 21, 23, 40, 55, 95); // 네이비 재킷
  c.fillRect(14, 15, 18, 20, 245, 245, 250); // 흰 와이셔츠
  c.fillRect(16, 16, 16, 20, 220, 45, 50); // 빨간 넥타이
  // 노란 수습 명찰
  c.fillRect(12, 17, 14, 19, 255, 215, 40);
  // 정장 바지 (다크 그레이)
  c.fillRect(12, 24, 15, 28, 30, 35, 50);
  c.fillRect(17, 24, 20, 28, 30, 35, 50);
  // 구두
  c.fillRect(11, 28, 15, 29, 20, 20, 25);
  c.fillRect(17, 28, 21, 29, 20, 20, 25);
  // 빗자루 도구 (오른손)
  c.fillRect(23, 8, 24, 26, 170, 110, 60); // 자루
  c.fillRect(21, 24, 26, 28, 235, 195, 80); // 솔
  c.fillRect(22, 29, 25, 29, 210, 165, 60);
  writeSprite('Assets/Sprites/Characters/player_brush.png', c, 32);
  writeSprite('Assets/Sprites/Characters/char_intern.png', c, 32);
}

// (B) char_adjuster (박사정 - 트렌치코트 + 중절모 + 돋보기 탐정)
function generateCharAdjuster() {
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 28, 9, 0, 0, 0, 70);
  // 중절모 (클래식 베이지/브라운)
  c.fillRect(9, 6, 23, 8, 160, 120, 75); // 모자 챙
  c.fillRect(12, 3, 20, 6, 140, 100, 60); // 모자 윗부분
  c.fillRect(12, 6, 20, 6, 70, 40, 30); // 모자 리본 띠
  // 얼굴
  c.fillRect(12, 9, 20, 14, 250, 210, 175);
  // 선글라스 / 분석경 안경
  c.fillRect(13, 10, 15, 12, 45, 90, 140);
  c.fillRect(17, 10, 19, 12, 45, 90, 140);
  c.set(16, 11, 30, 50, 80);
  // 트렌치코트 바디 (오피셜 버터 카멜)
  c.fillRect(10, 15, 22, 25, 185, 145, 95);
  c.fillRect(14, 15, 18, 19, 50, 45, 60); // 셔츠 이너
  c.fillRect(10, 20, 22, 21, 140, 105, 65); // 벨트
  c.set(15, 20, 255, 215, 50); c.set(16, 20, 255, 215, 50); // 황금 버클
  // 코트 하단 & 다리
  c.fillRect(11, 26, 15, 28, 60, 55, 65);
  c.fillRect(17, 26, 21, 28, 60, 55, 65);
  // 돋보기 / 데이터 스캐너 (오른손)
  c.fillCircle(24, 15, 4, 90, 190, 240); // 렌즈
  c.fillCircle(24, 15, 2, 200, 240, 255);
  c.strokeRect(21, 12, 27, 18, 220, 180, 50); // 황금 테두리
  c.fillRect(25, 18, 27, 22, 130, 85, 45); // 손잡이
  writeSprite('Assets/Sprites/Characters/char_adjuster.png', c, 32);
}

// (C) char_cleaner (강반장 - 주황색 안전조끼 & 헤드셋 & 대형 압축 청소포)
function generateCharCleaner() {
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 28, 10, 0, 0, 0, 80);
  // 헤드셋 & 머리
  c.fillRect(11, 4, 21, 8, 45, 45, 50); // 짧은 스포츠머리
  c.fillRect(9, 7, 10, 11, 245, 210, 50); // 헤드폰 귀마개 (노랑)
  c.fillRect(22, 7, 23, 11, 245, 210, 50);
  c.fillRect(10, 3, 22, 4, 80, 80, 90); // 헤드폰 밴드
  // 다부진 얼굴 & 수염 자국
  c.fillRect(11, 9, 21, 14, 245, 200, 165);
  c.set(13, 10, 30, 30, 35); c.set(19, 10, 30, 30, 35);
  c.fillRect(14, 13, 18, 14, 180, 150, 130); // 콧수염
  // 안전 방호 조끼 (오렌지 형광 & 실버 반사띠)
  c.fillRect(9, 15, 23, 23, 250, 110, 30);
  c.fillRect(9, 18, 23, 19, 240, 245, 255); // 반사띠
  c.fillRect(14, 15, 18, 23, 40, 85, 120); // 내부 청색 작업복
  // 튼튼한 카고 바지
  c.fillRect(10, 24, 15, 28, 65, 80, 70);
  c.fillRect(17, 24, 22, 28, 65, 80, 70);
  // 안전화 (스틸토 부츠)
  c.fillRect(9, 28, 15, 30, 140, 95, 45);
  c.fillRect(17, 28, 23, 30, 140, 95, 45);
  // 대형 청소 장비 슬링
  c.fillRect(5, 12, 8, 24, 50, 150, 210);
  c.fillRect(6, 10, 7, 12, 255, 215, 40);
  writeSprite('Assets/Sprites/Characters/char_cleaner.png', c, 32);
}

// (D) char_director (윤이사 - 명품 버건디 정장 + 황금 왕관 핀 + 서류가방)
function generateCharDirector() {
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 28, 9, 0, 0, 0, 75);
  // 금발 올백 헤어 & 황금 핀
  c.fillRect(11, 4, 21, 8, 235, 195, 75);
  c.fillRect(12, 3, 20, 4, 255, 225, 115);
  c.set(19, 4, 255, 255, 255); // 황금 왕관 핀 반짝임
  // 얼굴
  c.fillRect(12, 9, 20, 14, 255, 220, 190);
  c.set(14, 10, 45, 75, 130); c.set(18, 10, 45, 75, 130); // 푸른 눈
  c.set(15, 12, 210, 90, 100);
  // 럭셔리 버건디 더블 정장
  c.fillRect(10, 15, 22, 23, 145, 30, 60);
  c.fillRect(14, 15, 18, 19, 255, 255, 255); // 실크 셔츠
  c.fillRect(15, 16, 17, 22, 225, 180, 45); // 황금 타이
  // 황금 단추
  c.set(13, 19, 255, 215, 50); c.set(19, 19, 255, 215, 50);
  c.set(13, 22, 255, 215, 50); c.set(19, 22, 255, 215, 50);
  // 정장 슬랙스
  c.fillRect(11, 24, 15, 28, 110, 20, 45);
  c.fillRect(17, 24, 21, 28, 110, 20, 45);
  // 명품 가죽 구두
  c.fillRect(10, 28, 15, 29, 40, 20, 15);
  c.fillRect(17, 28, 22, 29, 40, 20, 15);
  // 고급 가죽 브리프케이스 (오른손)
  c.fillRect(23, 16, 30, 24, 180, 115, 45);
  c.strokeRect(23, 16, 30, 24, 120, 70, 25);
  c.fillRect(25, 15, 28, 16, 240, 190, 60); // 황금 손잡이
  writeSprite('Assets/Sprites/Characters/char_director.png', c, 32);
}

generateCharIntern();
generateCharAdjuster();
generateCharCleaner();
generateCharDirector();

// ==============================================================
// 2. 3대 정화 던전 맵 테마 배경 타일 (32x32 Seamless Tiles)
// ==============================================================

// (A) 도파민 격리 병동 (bg_hospital_floor.png) - 에메랄드 민트 타일 & 십자가 마크
function generateHospitalFloor() {
  const c = makeCanvas(32, 32);
  c.fillRect(0, 0, 31, 31, 220, 240, 235); // 맑고 시원한 민트 병원 바닥
  // 타일 그루브 라인 (격자)
  c.fillRect(0, 0, 31, 0, 180, 210, 205);
  c.fillRect(0, 0, 0, 31, 180, 210, 205);
  c.fillRect(0, 31, 31, 31, 150, 185, 180);
  c.fillRect(31, 0, 31, 31, 150, 185, 180);
  c.fillRect(16, 0, 16, 31, 190, 220, 215);
  c.fillRect(0, 16, 31, 16, 190, 220, 215);

  // 중앙 은은한 십자가 정화 패턴
  c.fillRect(14, 6, 18, 10, 160, 215, 200);
  c.fillRect(6, 14, 10, 18, 160, 215, 200);
  c.fillRect(22, 14, 26, 18, 160, 215, 200);
  c.fillRect(14, 22, 18, 26, 160, 215, 200);
  // 광택 스펙큘러
  c.set(2, 2, 255, 255, 255); c.set(3, 2, 240, 255, 250);
  c.set(18, 2, 255, 255, 255);
  c.set(2, 18, 255, 255, 255);
  writeSprite('Assets/Sprites/Environment/bg_hospital_floor.png', c, 32);
}

// (B) 번아웃 오피스 미궁 (bg_office_carpet.png) - 딥 블루/퍼플 카페트 & 서류 포스트잇
function generateOfficeCarpet() {
  const c = makeCanvas(32, 32);
  // 텍스처 카페트 패턴
  for (let y = 0; y < 32; y++) {
    for (let x = 0; x < 32; x++) {
      const noise = ((x * 13 + y * 7) % 5);
      c.set(x, y, 45 + noise * 4, 38 + noise * 3, 65 + noise * 5);
    }
  }
  // 카페트 격자 테두리
  c.strokeRect(0, 0, 31, 31, 35, 30, 50);
  // 바닥에 흩어진 포스트잇 메모 & 펜
  c.fillRect(8, 7, 13, 12, 255, 230, 100); // 노란 포스트잇
  c.fillRect(9, 8, 12, 8, 210, 180, 50); // 메모 줄
  c.fillRect(9, 10, 11, 10, 210, 180, 50);

  c.fillRect(20, 18, 25, 23, 255, 140, 170); // 핑크 포스트잇
  c.fillRect(21, 19, 24, 19, 210, 90, 120);

  c.fillRect(5, 23, 11, 24, 60, 170, 240); // 떨어진 파란 볼펜
  c.set(12, 23, 220, 220, 230);
  writeSprite('Assets/Sprites/Environment/bg_office_carpet.png', c, 32);
}

// (C) 심야 숏폼 고속도로 (bg_highway_road.png) - 아스팔트 + 네온 차선 & 디지털 글리치
function generateHighwayRoad() {
  const c = makeCanvas(32, 32);
  // 다크 사이버 아스팔트
  for (let y = 0; y < 32; y++) {
    for (let x = 0; x < 32; x++) {
      const n = (x * 17 + y * 23) % 7;
      c.set(x, y, 22 + n * 2, 20 + n * 2, 32 + n * 3);
    }
  }
  // 고속도로 중앙 차선 (황금 네온 발광)
  c.fillRect(15, 2, 17, 10, 255, 215, 40);
  c.fillRect(15, 18, 17, 26, 255, 215, 40);
  c.set(16, 4, 255, 255, 220); c.set(16, 20, 255, 255, 220); // 발광 코어

  // 양쪽 도로 갓길 네온 라인 (사이안 & 마젠타 숏폼 컬러)
  c.fillRect(1, 0, 2, 31, 0, 220, 255); // 틱톡/릴스 사이안
  c.fillRect(29, 0, 30, 31, 255, 40, 120); // 유튜브 쇼츠 마젠타
  writeSprite('Assets/Sprites/Environment/bg_highway_road.png', c, 32);
}

generateHospitalFloor();
generateOfficeCarpet();
generateHighwayRoad();

// ==============================================================
// 3. 본부(Town) & 인게임 공용 UI 스프라이트 (버튼, 패널, 슬라이더)
// ==============================================================

// (A) 도트 9-슬라이스 버튼 (ui_button_normal.png, ui_button_hover.png, ui_button_accent.png)
function generateUiButtons() {
  // 기본 버튼 (32x32)
  const cNormal = makeCanvas(32, 32);
  cNormal.fillRect(1, 1, 30, 30, 55, 60, 85); // 베이스
  cNormal.strokeRect(1, 1, 30, 30, 30, 32, 48); // 외곽 어두운 선
  cNormal.fillRect(2, 2, 29, 3, 110, 120, 160); // 상단 하이라이트
  cNormal.fillRect(2, 28, 29, 29, 35, 38, 55); // 하단 림
  writeSprite('Assets/Sprites/UI/ui_button_normal.png', cNormal, 32);

  // 강조 버튼 (아쿠아 민트 클린)
  const cAccent = makeCanvas(32, 32);
  cAccent.fillRect(1, 1, 30, 30, 60, 175, 140);
  cAccent.strokeRect(1, 1, 30, 30, 25, 95, 75);
  cAccent.fillRect(2, 2, 29, 3, 140, 235, 200);
  cAccent.fillRect(2, 28, 29, 29, 35, 125, 95);
  writeSprite('Assets/Sprites/UI/ui_button_accent.png', cAccent, 32);

  // 출동/위험 버튼 (레드/코랄)
  const cDanger = makeCanvas(32, 32);
  cDanger.fillRect(1, 1, 30, 30, 225, 75, 85);
  cDanger.strokeRect(1, 1, 30, 30, 140, 35, 45);
  cDanger.fillRect(2, 2, 29, 3, 255, 145, 155);
  cDanger.fillRect(2, 28, 29, 29, 160, 45, 55);
  writeSprite('Assets/Sprites/UI/ui_button_danger.png', cDanger, 32);

  // 레트로 패널 프레임 (ui_panel_frame.png, 48x48)
  const cPanel = makeCanvas(48, 48);
  cPanel.fillRect(0, 0, 47, 47, 30, 26, 42); // 딥 퍼플 이너
  cPanel.strokeRect(0, 0, 47, 47, 15, 12, 22); // 외곽 다크
  cPanel.strokeRect(2, 2, 45, 45, 85, 75, 115); // 메탈릭 브론즈/실버 테두리
  cPanel.strokeRect(4, 4, 43, 43, 45, 40, 62);
  // 코너 리벳 못
  cPanel.fillRect(5, 5, 7, 7, 245, 215, 100);
  cPanel.fillRect(40, 5, 42, 7, 245, 215, 100);
  cPanel.fillRect(5, 40, 7, 42, 245, 215, 100);
  cPanel.fillRect(40, 40, 42, 42, 245, 215, 100);
  writeSprite('Assets/Sprites/UI/ui_panel_frame.png', cPanel, 32);
}

generateUiButtons();

// ==============================================================
// 4. 보급소/창고 6종 장비 & 유물 도트 아이콘 (32x32)
// ==============================================================

// (A) item_first_aid.png (응급 구급약 키트)
{
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 27, 8, 0, 0, 0, 60);
  c.fillRect(7, 10, 25, 24, 240, 240, 245); // 화이트 박스
  c.strokeRect(7, 10, 25, 24, 180, 185, 195);
  c.fillRect(12, 7, 20, 10, 210, 45, 60); // 상단 빨간 손잡이
  c.fillRect(14, 8, 18, 9, 0, 0, 0, 0);
  // 전면 적십자
  c.fillRect(14, 13, 18, 21, 230, 40, 55);
  c.fillRect(10, 15, 22, 19, 230, 40, 55);
  writeSprite('Assets/Sprites/Items/item_first_aid.png', c, 32);
}

// (B) item_revive_policy.png (비상 소생 키트 주사기)
{
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 27, 8, 0, 0, 0, 60);
  // 황금 앰플 주사기
  c.fillRect(12, 10, 20, 22, 120, 220, 240); // 맑은 형광 블루 시약
  c.strokeRect(12, 10, 20, 22, 230, 235, 245);
  c.fillRect(14, 4, 18, 8, 240, 200, 60); // 플런저
  c.fillRect(15, 23, 17, 28, 200, 205, 215); // 바늘
  c.set(16, 29, 255, 255, 255);
  // 하이라이트
  c.fillRect(13, 12, 14, 20, 255, 255, 255);
  writeSprite('Assets/Sprites/Items/item_revive_policy.png', c, 32);
}

// (C) item_speed_booster.png (고카페인 에너지 드링크)
{
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 27, 8, 0, 0, 0, 60);
  // 캔 바디 (네온 라임 & 번개)
  c.fillRect(10, 8, 22, 25, 65, 215, 95);
  c.strokeRect(10, 8, 22, 25, 40, 140, 60);
  c.fillRect(11, 7, 21, 8, 210, 215, 225); // 캔 상단 림
  c.fillRect(11, 24, 21, 25, 180, 185, 195);
  // 황금 번개 마크 ⚡
  c.set(16, 11, 255, 235, 50); c.set(17, 11, 255, 235, 50);
  c.set(15, 13, 255, 235, 50); c.set(16, 13, 255, 235, 50);
  c.fillRect(14, 15, 18, 16, 255, 235, 50);
  c.set(15, 17, 255, 235, 50);
  c.set(14, 19, 255, 235, 50);
  writeSprite('Assets/Sprites/Items/item_speed_booster.png', c, 32);
}

// (D) artifact_golden_calc.png (황금 계산기)
{
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 27, 9, 0, 0, 0, 65);
  c.fillRect(9, 7, 23, 25, 245, 185, 45); // 황금 몸체
  c.strokeRect(9, 7, 23, 25, 165, 115, 25);
  // LCD 화면
  c.fillRect(11, 9, 21, 13, 140, 200, 160);
  c.fillRect(13, 11, 19, 11, 40, 70, 50); // 숫자 표시
  // 키패드 버튼들
  for (let row = 0; row < 3; row++) {
    for (let col = 0; col < 3; col++) {
      c.fillRect(11 + col * 4, 15 + row * 3, 13 + col * 4, 16 + row * 3, 120, 75, 20);
    }
  }
  // 반짝이
  c.set(22, 6, 255, 255, 255);
  writeSprite('Assets/Sprites/Items/artifact_golden_calc.png', c, 32);
}

// (E) artifact_magnet_badge.png (도파민 견인 배지)
{
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 27, 8, 0, 0, 0, 60);
  // 황금 별 배지
  c.fillCircle(16, 16, 9, 240, 175, 35);
  c.fillCircle(16, 16, 7, 255, 215, 75);
  // 자석 음각
  c.fillRect(12, 12, 14, 19, 220, 50, 50);
  c.fillRect(18, 12, 20, 19, 50, 100, 220);
  c.fillRect(14, 17, 18, 19, 220, 50, 50);
  writeSprite('Assets/Sprites/Items/artifact_magnet_badge.png', c, 32);
}

// (F) equip_coffee_thermos.png (철야 보온병)
{
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 27, 8, 0, 0, 0, 60);
  // 보온병 실린더 바디 (매트 블랙 & 오렌지 밴드)
  c.fillRect(11, 9, 21, 24, 50, 50, 55);
  c.strokeRect(11, 9, 21, 24, 25, 25, 30);
  c.fillRect(11, 15, 21, 17, 245, 120, 45); // 오렌지 실리콘 밴드
  // 뚜껑 컵 (스테인리스 실버)
  c.fillRect(12, 6, 20, 9, 210, 215, 225);
  // 김이 모락모락
  c.set(14, 3, 230, 235, 245);
  c.set(17, 2, 230, 235, 245);
  writeSprite('Assets/Sprites/Items/equip_coffee_thermos.png', c, 32);
}

console.log('🎉 All game sprites (Characters, Environments, UI, Items) generated cleanly!');
