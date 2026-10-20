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

function makeCanvas(w = 32, h = 32, bgR = 0, bgG = 0, bgB = 0, bgA = 0) {
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

function writeSprite(relPath, canvas) {
  const pngData = createPng(canvas.w, canvas.h, canvas.buf);
  const targets = [
    path.join(__dirname, '..', relPath),
    path.join(__dirname, '..', relPath.replace('Assets/Sprites/', 'Assets/Resources/'))
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
  console.log('Saved abyss sprite:', path.basename(relPath));
}

// 1. bg_abyss_matrix.png (스마트폰 알고리즘 심연 배경: 딥 네이비/바이올렛 + 사이버 매트릭스 그리드)
{
  const c = makeCanvas(32, 32, 16, 12, 28, 255); // 베이스 딥 다크 바이올렛
  // 디지털 그리드 라인
  for (let i = 0; i < 32; i += 8) {
    for (let x = 0; x < 32; x++) {
      c.set(x, i, 38, 26, 68, 255);
      c.set(i, x, 38, 26, 68, 255);
    }
  }
  // 네온 마젠타 & 시안 데이터 노드
  c.set(0, 0, 180, 50, 240);
  c.set(16, 16, 0, 220, 255);
  c.set(8, 24, 255, 60, 180);
  c.set(24, 8, 120, 80, 255);
  // 디지털 노이즈 픽셀
  c.set(5, 12, 50, 35, 90);
  c.set(20, 27, 50, 35, 90);
  c.set(11, 4, 70, 45, 120);
  writeSprite('Assets/Sprites/Environment/bg_abyss_matrix.png', c);
}

// 2. enemy_shorts.png (쇼츠 중독자: 스마트폰을 들고 붉은 안광을 뿜으며 고속 돌진하는 몬스터)
{
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 27, 8, 0, 0, 0, 60);
  // 후드티 몸체 (네온 오렌지 / 번갯빛)
  c.fillRect(10, 12, 22, 23, 235, 90, 30);
  c.strokeRect(10, 12, 22, 23, 160, 50, 20);
  // 얼굴/후드 안쪽 어둠
  c.fillRect(12, 6, 20, 14, 40, 25, 45);
  // 번뜩이는 중독된 붉은 눈 (화면 불빛 반사)
  c.set(14, 9, 255, 40, 40);
  c.set(18, 9, 255, 40, 40);
  // 손에 쥔 스마트폰 (세로형 화면에서 뿜어져 나오는 강한 빛)
  c.fillRect(14, 15, 18, 24, 25, 25, 35);
  c.fillRect(15, 16, 17, 23, 255, 60, 120); // 숏폼 핑크빛 비디오 화면
  c.set(16, 19, 255, 255, 255); // 재생 버튼
  // 돌진 잔상 스파크
  c.set(6, 16, 255, 180, 40);
  c.set(7, 20, 255, 180, 40);
  writeSprite('Assets/Sprites/Enemies/enemy_shorts.png', c);
}

// 3. enemy_spam.png (스팸 악플 슈터: 메가폰과 독성 말풍선을 뿜어내는 비행 드론형 몬스터)
{
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 27, 7, 0, 0, 0, 50);
  // 독성 보라/마젠타 비행 구체
  c.fillCircle(16, 15, 9, 140, 40, 180);
  c.fillCircle(16, 15, 7, 200, 60, 230);
  c.fillCircle(16, 15, 4, 255, 120, 255);
  // 악의적인 눈매
  c.fillRect(12, 12, 15, 14, 255, 255, 255);
  c.fillRect(17, 12, 20, 14, 255, 255, 255);
  c.set(13, 13, 180, 20, 40);
  c.set(18, 13, 180, 20, 40);
  // 메가폰 확성기 입구
  c.fillRect(14, 18, 18, 22, 50, 50, 65);
  c.fillRect(13, 22, 19, 24, 255, 50, 50);
  writeSprite('Assets/Sprites/Enemies/enemy_spam.png', c);
}

// 4. enemy_doomscroller.png (둠스크롤러 골렘: 거대 스톤/사이버 블랙홀 골렘)
{
  const c = makeCanvas(32, 32);
  c.fillCircle(16, 28, 11, 0, 0, 0, 70);
  // 거대한 바위 골렘 바디
  c.fillCircle(16, 16, 12, 45, 30, 65);
  c.fillCircle(16, 16, 9, 85, 50, 120);
  // 가슴 중앙의 블랙홀/싱크홀 흡입 소용돌이 코어
  c.fillCircle(16, 16, 6, 20, 10, 35);
  c.fillCircle(16, 16, 4, 180, 50, 255);
  c.fillCircle(16, 16, 2, 255, 255, 255); // 특이점 코어
  // 양 어깨 거대 암석
  c.fillRect(4, 8, 9, 16, 70, 45, 95);
  c.fillRect(23, 8, 28, 16, 70, 45, 95);
  // 무서운 2개의 보랏빛 눈
  c.set(12, 9, 255, 220, 80);
  c.set(20, 9, 255, 220, 80);
  writeSprite('Assets/Sprites/Enemies/enemy_doomscroller.png', c);
}

console.log('🎉 Successfully generated all Abyss sprites!');
