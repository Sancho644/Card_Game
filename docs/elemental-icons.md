# Иконки стихий — выбранный набор

Выбор игрока: земля — 2, огонь — 1, вода — 3, ветер — 3, свет — 2, тьма — 3.

| Стихия | Рисунок | Ассет |
|---|---|---|
| Земля | Кристаллы | `Assets/Art/Icons/element-earth-v2.png` |
| Огонь | Пламя | `Assets/Art/Icons/element-fire-v2.png` |
| Вода | Капля с завитком | `Assets/Art/Icons/element-water-v2.png` |
| Ветер | Вихрь | `Assets/Art/Icons/element-wind-v2.png` |
| Свет | Четырёхлучевая звезда | `Assets/Art/Icons/element-light-v2.png` |
| Тьма | Теневой водоворот | `Assets/Art/Icons/element-dark-v2.png` |

Исходники: отдельные цветные PNG с прозрачностью. Генерация выполнена встроенным image_gen. В Unity: Sprite Single, max 512, без mipmaps, без сжатия, alphaIsTransparency. Оригинальный размер хранится в PNG. Старые спрайты оставлены; VisualSettings ссылается на v2.

Иконки содержат собственный цвет. CreatureCardFaceView, CardView, CardZoomView и CollectionCardView отображают их с белым множителем; затемнение игровой карты применяется отдельно. Тонирование текста и цветных полос сохранено.

## Набор промптов

Референс: `docs/art-references/elemental-icon-concepts-v1.png` (строки: земля, огонь, вода, ветер, свет, тьма; столбцы 1–3). Каждый спрайт создан отдельным запросом с transparent_background=true.

### Земля

```text
Use case: stylized-concept. Asset type: standalone Unity elemental icon sprite. Input image 1 is a CONCEPT REFERENCE SHEET only. Recreate ONLY the specified selected icon, preserving its design and chunky painted fantasy style. Subject: row 1 EARTH (ЗЕМЛЯ), column 2. A compact cluster of three chunky ochre and golden rock crystals growing from a small dark brown stone base. Preserve the selected faceted crystal silhouette. Composition: one single centered icon on a square canvas, filling about 82 percent, generous even transparent margin. Crisp readable silhouette at 48 pixels, few bold shapes, tactile hand-painted facets, upper-left highlights, dark saturated edge shading. Same family as colorful painted fantasy card frames. Actual fully transparent background, including outside contour. No background color, no checkerboard baked into pixels, no frame, no badge, no circle plate, no labels, no numbers, no letters, no sheet, no additional icons, no watermark, no floating particles, no outer glow. Keep the icon itself opaque except natural empty spaces. This must be a usable final game sprite.
```

### Огонь

```text
Use case: stylized-concept. Asset type: standalone Unity elemental icon sprite. Input image 1 is a CONCEPT REFERENCE SHEET only. Recreate ONLY the specified selected icon, preserving its design and chunky painted fantasy style. Subject: row 2 FIRE (ОГОНЬ), column 1. A single bold orange flame silhouette with two side tongues and a golden yellow smaller inner flame, rich red orange outside. Preserve the selected flame shape. Composition: one single centered icon on a square canvas, filling about 82 percent, generous even transparent margin. Crisp readable silhouette at 48 pixels, few bold shapes, tactile hand-painted facets, upper-left highlights, dark saturated edge shading. Same family as colorful painted fantasy card frames. Actual fully transparent background, including outside contour. No background color, no checkerboard baked into pixels, no frame, no badge, no circle plate, no labels, no numbers, no letters, no sheet, no additional icons, no watermark, no floating particles, no outer glow. Keep the icon itself opaque except natural empty spaces. This must be a usable final game sprite.
```

### Вода

```text
Use case: stylized-concept. Asset type: standalone Unity elemental icon sprite. Input image 1 is a CONCEPT REFERENCE SHEET only. Recreate ONLY the specified selected icon, preserving its design and chunky painted fantasy style. Subject: row 3 WATER (ВОДА), column 3. A large glossy cobalt blue water droplet with an aqua curling ribbon of water wrapping its lower half, a compact circular sweep at the base. Preserve the selected drop and wave silhouette. Composition: one single centered icon on a square canvas, filling about 82 percent, generous even transparent margin. Crisp readable silhouette at 48 pixels, few bold shapes, tactile hand-painted facets, upper-left highlights, dark saturated edge shading. Same family as colorful painted fantasy card frames. Actual fully transparent background, including outside contour. No background color, no checkerboard baked into pixels, no frame, no badge, no circle plate, no labels, no numbers, no letters, no sheet, no additional icons, no watermark, no floating particles, no outer glow. Keep the icon itself opaque except natural empty spaces. This must be a usable final game sprite.
```

### Ветер

```text
Use case: stylized-concept. Asset type: standalone Unity elemental icon sprite. Input image 1 is a CONCEPT REFERENCE SHEET only. Recreate ONLY the specified selected icon, preserving its design and chunky painted fantasy style. Subject: row 4 WIND (ВЕТЕР), column 3. A compact mint jade whirlwind, three broad tapering ribbons circling into a small tornado, wider at top, slender at bottom. Preserve its selected tornado shape, clearly distinct from the droplet. Composition: one single centered icon on a square canvas, filling about 82 percent, generous even transparent margin. Crisp readable silhouette at 48 pixels, few bold shapes, tactile hand-painted facets, upper-left highlights, dark saturated edge shading. Same family as colorful painted fantasy card frames. Actual fully transparent background, including outside contour. No background color, no checkerboard baked into pixels, no frame, no badge, no circle plate, no labels, no numbers, no letters, no sheet, no additional icons, no watermark, no floating particles, no outer glow. Keep the icon itself opaque except natural empty spaces. This must be a usable final game sprite.
```

### Свет

```text
Use case: stylized-concept. Asset type: standalone Unity elemental icon sprite. Input image 1 is a CONCEPT REFERENCE SHEET only. Recreate ONLY the specified selected icon, preserving its design and chunky painted fantasy style. Subject: row 5 LIGHT (СВЕТ), column 2. A bold ivory golden FOUR-POINT star with long vertical and horizontal points and four shorter diagonal rays. Gold facets and warm ivory core. Preserve the selected four point star silhouette. Composition: one single centered icon on a square canvas, filling about 82 percent, generous even transparent margin. Crisp readable silhouette at 48 pixels, few bold shapes, tactile hand-painted facets, upper-left highlights, dark saturated edge shading. Same family as colorful painted fantasy card frames. Actual fully transparent background, including outside contour. No background color, no checkerboard baked into pixels, no frame, no badge, no circle plate, no labels, no numbers, no letters, no sheet, no additional icons, no watermark, no floating particles, no outer glow. Keep the icon itself opaque except natural empty spaces. This must be a usable final game sprite.
```

### Тьма

```text
Use case: stylized-concept. Asset type: standalone Unity elemental icon sprite. Input image 1 is a CONCEPT REFERENCE SHEET only. Recreate ONLY the specified selected icon, preserving its design and chunky painted fantasy style. Subject: row 6 DARK (ТЬМА), column 3. A near-black central void surrounded by three broad angular amethyst purple shadow arcs forming a compact rotating vortex with pointed tips. Violet lit edges ensure silhouette remains visible on a dark card. Preserve the selected shadow whirlpool shape. Composition: one single centered icon on a square canvas, filling about 82 percent, generous even transparent margin. Crisp readable silhouette at 48 pixels, few bold shapes, tactile hand-painted facets, upper-left highlights, dark saturated edge shading. Same family as colorful painted fantasy card frames. Actual fully transparent background, including outside contour. No background color, no checkerboard baked into pixels, no frame, no badge, no circle plate, no labels, no numbers, no letters, no sheet, no additional icons, no watermark, no floating particles, no outer glow. Keep the icon itself opaque except natural empty spaces. This must be a usable final game sprite.
```

## Проверка

Через Unity MCP прошли 262/262 EditMode-теста, включая проверку шести различных спрайтов, передачи нужной иконки в карту и сохранения цветов при затемнении/восстановлении.

Play mode: просмотрены все шесть стихий на общей странице коллекции, карты существ и поля в руке, увеличенная карта Каменного жука. Размеры, пропорции, прозрачность и цветное отображение корректны. Консоль Unity: 0 ошибок и предупреждений. Скриншоты проверки: `Temp/ArtPreview/selected-element-icons-collection.png`, `selected-element-icons-hand.png`, `selected-element-icons-zoom.png` (временные файлы, в Git не включены).

Ручная проверка: Bootstrap → коллекция → существа каждой стихии; затем матч и просмотр карты правой кнопкой/долгим нажатием. Проверить значок в правом верхнем углу, рисунок-заглушку у карт без арта и отсутствие цветной прямоугольной подложки вокруг PNG.
