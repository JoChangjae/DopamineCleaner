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

function makeCanvas(w, h) {
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
    fillCircle(cx, cy, radius, r, g, b, a = 255) {
      for (let y = -radius; y <= radius; y++) {
        for (let x = -radius; x <= radius; x++) {
          if (x * x + y * y <= radius * radius) {
            this.set(Math.round(cx + x), Math.round(cy + y), r, g, b, a);
          }
        }
      }
    },
    fillRect(x1, y1, x2, y2, r, g, b, a = 255) {
      for (let y = y1; y <= y2; y++) {
        for (let x = x1; x <= x2; x++) {
          this.set(x, y, r, g, b, a);
        }
      }
    }
  };
}

function writeSprite(relPath, canvas, ppu = 32) {
  const fullPath = path.join(__dirname, '..', relPath);
  fs.mkdirSync(path.dirname(fullPath), { recursive: true });
  fs.writeFileSync(fullPath, createPng(canvas.w, canvas.h, canvas.buf));

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
  if (!fs.existsSync(fullPath + '.meta')) {
    fs.writeFileSync(fullPath + '.meta', metaContent);
  }
  console.log('Saved sprite:', relPath);
}

// -------------------------------------------------------------
// 1. 고화질 32x32 직관적인 필드 드랍 아이템 스프라이트
// -------------------------------------------------------------

// (A) 케이크 (item_cake.png, 32x32) - 층층이 딸기 생크림 조각 케이크
{
  const c = makeCanvas(32, 32);
  // 그림자
  c.fillCircle(16, 26, 9, 0, 0, 0, 60);

  // 케이크 빵 바디 (골든 옐로우 스폰지)
  c.fillRect(7, 14, 25, 24, 245, 215, 160);
  // 딸기잼 샌드 레이어
  c.fillRect(7, 18, 25, 20, 235, 60, 95);
  // 하얀 생크림 프로스팅
  c.fillRect(6, 12, 26, 15, 255, 255, 255);
  c.fillRect(5, 14, 7, 17, 255, 255, 255); // 옆으로 흘러내리는 크림 드립
  c.fillRect(14, 14, 16, 17, 255, 255, 255);
  c.fillRect(23, 14, 25, 17, 255, 255, 255);

  // 상단 탐스러운 딸기 토핑
  c.fillCircle(16, 9, 4, 245, 45, 65);
  c.set(15, 8, 255, 160, 180); // 딸기 윤기 하이라이트
  c.fillRect(15, 5, 17, 6, 45, 140, 75); // 딸기 꼭지 잎

  // 외곽선 정리
  c.fillRect(6, 24, 26, 25, 180, 130, 80); // 바닥 테두리
  writeSprite('Assets/Sprites/Items/item_cake.png', c, 32);
}

// (B) 토마토 폭탄 (item_bomb.png, 32x32) - 빨갛고 동글동글한 토마토 폭탄 & 타오르는 심지
{
  const c = makeCanvas(32, 32);
  // 그림자
  c.fillCircle(16, 26, 9, 0, 0, 0, 60);

  // 붉은 토마토 몸체
  c.fillCircle(16, 18, 10, 235, 40, 40);
  c.fillCircle(15, 16, 8, 255, 75, 75);
  c.fillCircle(13, 14, 4, 255, 140, 140); // 반짝이는 하이라이트

  // 토마토 초록 꼭지 잎
  c.fillRect(12, 8, 20, 10, 45, 145, 75);
  c.fillRect(15, 7, 17, 11, 45, 145, 75);
  c.set(11, 9, 35, 115, 55);
  c.set(21, 9, 35, 115, 55);

  // 꼬불꼬불 폭탄 심지
  c.set(16, 6, 120, 90, 60);
  c.set(17, 5, 120, 90, 60);
  c.set(18, 4, 120, 90, 60);

  // 타오르는 불꽃 파티클
  c.fillCircle(19, 3, 2, 255, 215, 50);
  c.set(19, 3, 255, 255, 255);
  c.set(21, 2, 255, 90, 30);
  c.set(18, 1, 255, 150, 40);
  writeSprite('Assets/Sprites/Items/item_bomb.png', c, 32);
}

// (C) 슈퍼 자석 (item_magnet.png, 32x32) - 클래식 말굽 자석 & 전기 스파크
{
  const c = makeCanvas(32, 32);
  // 그림자
  c.fillCircle(16, 27, 9, 0, 0, 0, 60);

  // U자형 말굽 자석 - 왼쪽(N극 빨강), 오른쪽(S극 파랑)
  c.fillRect(8, 8, 13, 21, 235, 50, 50);  // N극 몸체
  c.fillRect(19, 8, 24, 21, 50, 100, 235); // S극 몸체
  c.fillRect(11, 18, 21, 24, 200, 40, 50); // 하단 U 커브
  c.fillRect(14, 12, 18, 18, 0, 0, 0, 0);  // 내부 빈 공간 뚫기

  // 은색 자석 팁
  c.fillRect(8, 6, 13, 8, 230, 235, 245);
  c.fillRect(19, 6, 24, 8, 230, 235, 245);
  c.fillRect(8, 5, 13, 5, 160, 170, 185);
  c.fillRect(19, 5, 24, 5, 160, 170, 185);

  // 반짝이는 전기 스파크 (노랑)
  c.set(10, 3, 255, 235, 80);
  c.set(11, 2, 255, 255, 180);
  c.set(21, 3, 255, 235, 80);
  c.set(22, 2, 255, 255, 180);
  c.set(16, 5, 255, 220, 60);
  writeSprite('Assets/Sprites/Items/item_magnet.png', c, 32);
}

// (D) 황금 보물상자 (item_chest.png, 32x32) - 눈부신 골드 보물상자
{
  const c = makeCanvas(32, 32);
  // 그림자
  c.fillCircle(16, 26, 10, 0, 0, 0, 60);

  // 상자 본체 (빛나는 황금 골드 & 원목 테두리)
  c.fillRect(6, 14, 26, 25, 240, 170, 40);
  c.fillRect(8, 16, 24, 23, 255, 210, 75); // 전면 하이라이트

  // 상자 뚜껑
  c.fillRect(5, 10, 27, 15, 220, 145, 30);
  c.fillRect(7, 9, 25, 13, 255, 225, 95);

  // 쇠 장식 띠 & 리벳
  c.fillRect(9, 9, 11, 25, 120, 75, 25);
  c.fillRect(21, 9, 23, 25, 120, 75, 25);
  c.set(10, 12, 255, 230, 120);
  c.set(22, 12, 255, 230, 120);
  c.set(10, 20, 255, 230, 120);
  c.set(22, 20, 255, 230, 120);

  // 중앙 열쇠구멍 자물쇠 (실버 & 반짝임)
  c.fillRect(14, 13, 18, 18, 255, 255, 255);
  c.set(16, 15, 60, 40, 20);
  c.set(16, 16, 60, 40, 20);

  // 반짝이 별
  c.set(26, 7, 255, 255, 255);
  c.set(25, 8, 255, 230, 100);
  writeSprite('Assets/Sprites/Items/item_chest.png', c, 32);
}

// -------------------------------------------------------------
// 2. 반짝이는 32x32 다이아몬드 크리스탈 보석 (Green, Blue, Pink, Mega)
// -------------------------------------------------------------
function makeCrystal(r, g, b, hr, hg, hb, size = 32) {
  const c = makeCanvas(size, size);
  const mid = size / 2;
  // 그림자
  c.fillCircle(mid, size - 5, Math.floor(mid * 0.6), 0, 0, 0, 50);

  // 다이아몬드 젬 컷팅 형태
  for (let y = 6; y < size - 6; y++) {
    const dy = Math.abs(y - mid);
    const span = Math.round((mid - 5) * (1 - dy / mid));
    for (let x = mid - span; x <= mid + span; x++) {
      if (x < mid) {
        c.set(x, y, r, g, b); // 어두운 면
      } else {
        c.set(x, y, Math.min(255, r + 25), Math.min(255, g + 25), Math.min(255, b + 25)); // 밝은 면
      }
    }
  }

  // 상단 하이라이트 패싯
  c.fillCircle(mid - 2, mid - 2, 3, hr, hg, hb);
  c.set(mid - 2, mid - 3, 255, 255, 255);
  c.set(mid - 3, mid - 2, 255, 255, 255);
  return c;
}

writeSprite('Assets/Sprites/Items/gem_green.png', makeCrystal(65, 175, 120, 160, 245, 195, 32), 32);
writeSprite('Assets/Sprites/Items/gem_blue.png', makeCrystal(60, 150, 230, 170, 225, 255, 32), 32);
writeSprite('Assets/Sprites/Items/gem_pink.png', makeCrystal(245, 85, 135, 255, 185, 215, 32), 32);

// 메가 핑크 다이아몬드 (무지개빛 골드 림)
{
  const c = makeCrystal(255, 50, 110, 255, 215, 235, 32);
  // 외곽 골든 반짝임
  c.set(16, 4, 255, 220, 80);
  c.set(4, 16, 255, 220, 80);
  c.set(27, 16, 255, 220, 80);
  c.set(16, 27, 255, 220, 80);
  writeSprite('Assets/Sprites/Items/gem_mega.png', c, 32);
}

console.log('✨ All 32x32 field drop item sprites regenerated successfully!');
