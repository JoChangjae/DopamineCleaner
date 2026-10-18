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

const w = 64;
const h = 64;
const buf = Buffer.alloc(w * h * 4, 0);

function set(x, y, r, g, b, a = 255) {
  x = ((x % w) + w) % w;
  y = ((y % h) + h) % h;
  const idx = (y * w + x) * 4;
  buf[idx] = r;
  buf[idx + 1] = g;
  buf[idx + 2] = b;
  buf[idx + 3] = a;
}

// 1. 체크무늬 파스텔 잔디 (32x32 타일 4개)
for (let y = 0; y < 64; y++) {
  for (let x = 0; x < 64; x++) {
    const isTileA = ((Math.floor(x / 32) + Math.floor(y / 32)) % 2 === 0);
    if (isTileA) {
      set(x, y, 236, 246, 240); // 부드러운 밀크그린
    } else {
      set(x, y, 224, 240, 230); // 산뜻한 민트그린
    }
  }
}

// 2. 타일 경계선 미세 음영 (1px 테두리)
for (let i = 0; i < 64; i++) {
  set(31, i, 215, 233, 223);
  set(i, 31, 215, 233, 223);
  set(63, i, 215, 233, 223);
  set(i, 63, 215, 233, 223);
}

// 데이지 꽃 그리기
function drawDaisy(cx, cy) {
  set(cx, cy - 2, 255, 255, 255);
  set(cx, cy + 2, 255, 255, 255);
  set(cx - 2, cy, 255, 255, 255);
  set(cx + 2, cy, 255, 255, 255);
  set(cx - 1, cy - 1, 255, 255, 255);
  set(cx + 1, cy - 1, 255, 255, 255);
  set(cx - 1, cy + 1, 255, 255, 255);
  set(cx + 1, cy + 1, 255, 255, 255);
  set(cx, cy, 255, 215, 80); // 노란 꽃술
}

// 클로버 잎
function drawClover(cx, cy) {
  set(cx, cy - 1, 100, 195, 135);
  set(cx, cy + 1, 100, 195, 135);
  set(cx - 1, cy, 100, 195, 135);
  set(cx + 1, cy, 100, 195, 135);
  set(cx, cy, 70, 160, 100);
  set(cx, cy + 2, 70, 160, 100);
}

// 반짝이 별 / 이슬방울
function drawSparkle(cx, cy, r, g, b) {
  set(cx, cy, 255, 255, 255);
  set(cx - 1, cy, r, g, b);
  set(cx + 1, cy, r, g, b);
  set(cx, cy - 1, r, g, b);
  set(cx, cy + 1, r, g, b);
}

// 잔디 디테일
function drawGrassBlade(cx, cy) {
  set(cx, cy, 185, 218, 195);
  set(cx + 1, cy - 1, 185, 218, 195);
  set(cx + 2, cy - 2, 200, 228, 210);
}

// 오브젝트 분산 배치
drawDaisy(14, 12);
drawClover(24, 22);
drawGrassBlade(6, 26);

drawSparkle(48, 14, 255, 205, 120);
drawDaisy(52, 22);
drawGrassBlade(38, 8);

drawSparkle(12, 44, 140, 210, 255);
drawClover(22, 52);
drawGrassBlade(28, 38);

drawDaisy(46, 50);
drawGrassBlade(56, 42);
drawSparkle(38, 56, 255, 165, 205);

const targetDir = path.join(__dirname, '../Assets/Sprites/Environment');
fs.mkdirSync(targetDir, { recursive: true });
const pngPath = path.join(targetDir, 'bg_mental_garden.png');
fs.writeFileSync(pngPath, createPng(w, h, buf));

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
    wrapU: 0
    wrapV: 0
    wrapW: 0
  spriteMode: 1
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 32
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
`;
fs.writeFileSync(pngPath + '.meta', meta);
console.log('✨ bg_mental_garden.png generated successfully at:', pngPath);
