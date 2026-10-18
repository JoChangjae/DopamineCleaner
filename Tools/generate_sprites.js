const fs = require('fs');
const path = require('path');
const zlib = require('zlib');
const crypto = require('crypto');

// 1. PNG 생성 헬퍼
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
  const fullPath = path.join(__dirname, relPath);
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
  fs.writeFileSync(fullPath + '.meta', metaContent);
  console.log('Saved:', relPath);
}

// -------------------------------------------------------------
// 2. 도파민 클리너 스프라이트 디자인
// -------------------------------------------------------------

// (1) 플레이어: 귀여운 스마트 브러시 요정 (32x32)
{
  const c = makeCanvas(32, 32);
  // 몸통 (파스텔 핑크/스트로베리 젤리형)
  c.fillCircle(16, 17, 10, 255, 117, 143);
  c.fillCircle(16, 16, 9, 255, 143, 163);

  // 헤어밴드 / 꼭지 (산뜻한 민트 초록)
  c.fillCircle(16, 7, 3, 45, 106, 79);
  c.fillRect(14, 5, 18, 7, 82, 183, 136);

  // 눈 (초롱초롱한 블랙 & 화이트 하이라이트)
  c.fillRect(12, 15, 13, 18, 40, 30, 45);
  c.fillRect(19, 15, 20, 18, 40, 30, 45);
  c.set(12, 15, 255, 255, 255);
  c.set(19, 15, 255, 255, 255);

  // 발그레한 볼터치 (피치 핑크)
  c.fillRect(10, 19, 12, 20, 255, 80, 110);
  c.fillRect(20, 19, 22, 20, 255, 80, 110);

  // 귀여운 청소용 버블 브러시 (오른손)
  c.fillRect(24, 18, 28, 20, 230, 180, 90); // 손잡이
  c.fillCircle(28, 14, 3, 120, 220, 255);  // 비누 거품 헤드
  c.set(29, 13, 255, 255, 255);
  writeSprite('Assets/Sprites/Characters/player_brush.png', c, 32);
}

// (2) 몬스터: 잡생각 먼지 (Mind Dust, 32x32)
{
  const c = makeCanvas(32, 32);
  // 몽글몽글 먼지 구름 외곽
  c.fillCircle(16, 16, 9, 214, 199, 232);
  c.fillCircle(11, 15, 6, 200, 180, 225);
  c.fillCircle(21, 15, 6, 200, 180, 225);
  c.fillCircle(16, 21, 6, 185, 165, 215);
  c.fillCircle(16, 11, 6, 225, 215, 240);

  // 어지러운 소용돌이 눈
  c.fillRect(13, 14, 14, 16, 90, 70, 120);
  c.fillRect(18, 14, 19, 16, 90, 70, 120);
  c.set(13, 14, 255, 255, 255);
  c.set(18, 14, 255, 255, 255);
  // 삐쭉빼쭉 먼지 가닥
  c.set(7, 10, 190, 170, 220);
  c.set(24, 10, 190, 170, 220);
  c.set(8, 22, 190, 170, 220);
  c.set(23, 22, 190, 170, 220);
  writeSprite('Assets/Sprites/Enemies/enemy_dust.png', c, 32);
}

// (3) 몬스터: 쏟아지는 졸음 (Heavy Sloth, 32x32)
{
  const c = makeCanvas(32, 32);
  // 포근한 하늘빛 슬라임
  c.fillCircle(16, 18, 10, 190, 225, 230);
  c.fillRect(8, 18, 24, 26, 190, 225, 230);
  c.fillCircle(16, 17, 8, 210, 240, 245);

  // 감긴 눈 (Zzz 표정)
  c.fillRect(12, 17, 14, 18, 50, 90, 130);
  c.fillRect(18, 17, 20, 18, 50, 90, 130);

  // 수면 모자 (Nightcap, 부드러운 바이올렛)
  c.fillRect(12, 6, 20, 10, 160, 150, 210);
  c.fillRect(18, 3, 23, 6, 160, 150, 210);
  c.fillCircle(24, 3, 2, 255, 255, 255); // 방울
  writeSprite('Assets/Sprites/Enemies/enemy_sloth.png', c, 32);
}

// (4) 몬스터: 알림 유령 (Notification Ghost, 32x32)
{
  const c = makeCanvas(32, 32);
  // 스마트폰 핑크빛 유령 몸체
  c.fillCircle(16, 13, 9, 255, 180, 200);
  c.fillRect(8, 13, 24, 23, 255, 180, 200);
  // 유령 하단 꼬리 파동
  c.fillCircle(10, 23, 2, 255, 180, 200);
  c.fillCircle(16, 24, 3, 255, 180, 200);
  c.fillCircle(22, 23, 2, 255, 180, 200);

  // 붉은 알림 뱃지 (!) 마크
  c.fillCircle(22, 8, 4, 255, 60, 80);
  c.set(22, 6, 255, 255, 255);
  c.set(22, 7, 255, 255, 255);
  c.set(22, 9, 255, 255, 255);

  // 반짝이는 큰 눈
  c.fillRect(12, 13, 14, 16, 80, 30, 50);
  c.fillRect(18, 13, 20, 16, 80, 30, 50);
  c.set(13, 14, 255, 255, 255);
  c.set(19, 14, 255, 255, 255);
  writeSprite('Assets/Sprites/Enemies/enemy_ghost.png', c, 32);
}

// (5) 몬스터: 도파민 거인 보스 (Dopamine Boss, 64x64)
{
  const c = makeCanvas(64, 64);
  // 거대한 사이버 퍼플 보스 몸체
  c.fillCircle(32, 34, 22, 123, 124, 219);
  c.fillCircle(32, 32, 19, 145, 146, 235);

  // 번쩍이는 황금 왕관
  c.fillRect(24, 10, 40, 16, 255, 210, 70);
  c.fillRect(22, 6, 26, 12, 255, 210, 70);
  c.fillRect(30, 4, 34, 12, 255, 230, 100);
  c.fillRect(38, 6, 42, 12, 255, 210, 70);
  // 왕관 보석
  c.set(32, 8, 255, 50, 80);
  c.set(24, 9, 50, 180, 255);
  c.set(40, 9, 50, 180, 255);

  // 위압적인 붉은 보스 눈
  c.fillRect(23, 28, 28, 33, 255, 60, 90);
  c.fillRect(36, 28, 41, 33, 255, 60, 90);
  c.fillRect(25, 30, 27, 32, 255, 255, 255);
  c.fillRect(38, 30, 40, 255, 255, 255);

  // 사악한 보스 미소
  c.fillRect(28, 41, 36, 43, 60, 40, 90);
  writeSprite('Assets/Sprites/Enemies/enemy_boss.png', c, 32);
}

// (6) 경험치 보석 4종 (16x16, 24x24)
{
  function makeGem(r, g, b, hr, hg, hb, size = 16) {
    const c = makeCanvas(size, size);
    const mid = size / 2;
    c.fillCircle(mid, mid, mid - 2, r, g, b);
    c.fillCircle(mid - 1, mid - 1, mid - 4, hr, hg, hb);
    c.set(Math.floor(mid - 1), Math.floor(mid - 2), 255, 255, 255);
    return c;
  }
  writeSprite('Assets/Sprites/Items/gem_green.png', makeGem(82, 183, 136, 120, 220, 170, 16), 16);
  writeSprite('Assets/Sprites/Items/gem_blue.png', makeGem(78, 168, 222, 120, 200, 250, 16), 16);
  writeSprite('Assets/Sprites/Items/gem_pink.png', makeGem(255, 117, 143, 255, 160, 180, 16), 16);
  writeSprite('Assets/Sprites/Items/gem_mega.png', makeGem(255, 60, 110, 255, 210, 230, 24), 24);
}

// (7) 특수 아이템 4종 (24x24)
{
  // 케이크
  const cake = makeCanvas(24, 24);
  cake.fillRect(5, 10, 19, 18, 255, 235, 215); // 빵 시트
  cake.fillRect(5, 10, 19, 12, 255, 255, 255); // 크림
  cake.fillRect(5, 14, 19, 15, 255, 120, 150); // 딸기잼 레이어
  cake.fillCircle(12, 8, 3, 255, 60, 80);      // 위쪽 딸기
  writeSprite('Assets/Sprites/Items/item_cake.png', cake, 24);

  // 토마토 폭탄
  const bomb = makeCanvas(24, 24);
  bomb.fillCircle(12, 14, 8, 255, 60, 60);
  bomb.fillCircle(10, 12, 6, 255, 100, 100);
  bomb.fillRect(10, 5, 14, 8, 50, 140, 70); // 꼭지
  bomb.set(14, 4, 255, 200, 50); // 심지 불꽃
  writeSprite('Assets/Sprites/Items/item_bomb.png', bomb, 24);

  // 슈퍼 자석
  const mag = makeCanvas(24, 24);
  mag.fillRect(5, 6, 8, 17, 230, 70, 70);   // 왼쪽 극 (레드)
  mag.fillRect(16, 6, 19, 17, 70, 120, 230); // 오른쪽 극 (블루)
  mag.fillRect(8, 14, 16, 17, 200, 200, 200); // 하단 U 연결
  mag.fillRect(5, 5, 8, 7, 255, 255, 255);  // 은색 팁
  mag.fillRect(16, 5, 19, 7, 255, 255, 255);
  writeSprite('Assets/Sprites/Items/item_magnet.png', mag, 24);

  // 황금 보물상자
  const chest = makeCanvas(24, 24);
  chest.fillRect(4, 9, 20, 20, 255, 190, 50); // 골드 몸체
  chest.fillRect(6, 11, 18, 18, 220, 150, 30);
  chest.fillRect(4, 13, 20, 14, 100, 60, 20);  // 경계선
  chest.fillRect(11, 12, 13, 16, 255, 255, 255); // 자물쇠
  writeSprite('Assets/Sprites/Items/item_chest.png', chest, 24);
}

// (8) 무기 & 투사체 (16x16, 20x20)
{
  // 클린 탄환
  const bullet = makeCanvas(16, 16);
  bullet.fillCircle(8, 8, 5, 255, 117, 143);
  bullet.fillCircle(7, 7, 3, 255, 180, 200);
  bullet.set(6, 6, 255, 255, 255);
  writeSprite('Assets/Sprites/Weapons/bullet_clean.png', bullet, 16);

  // 기가 개틀링 탄환
  const gatling = makeCanvas(16, 16);
  gatling.fillCircle(8, 8, 6, 255, 210, 50);
  gatling.fillCircle(8, 8, 4, 255, 245, 150);
  gatling.set(8, 8, 255, 255, 255);
  writeSprite('Assets/Sprites/Weapons/bullet_gatling.png', gatling, 16);

  // 위성 배리어
  const sat = makeCanvas(20, 20);
  sat.fillCircle(10, 10, 7, 255, 140, 170);
  sat.fillCircle(9, 9, 5, 255, 180, 210);
  sat.fillRect(7, 2, 13, 4, 70, 180, 110); // 딸기 행성 잎
  writeSprite('Assets/Sprites/Weapons/satellite_orbit.png', sat, 20);
}

console.log('✨ All 2D Pixel Art Sprites generated successfully!');
