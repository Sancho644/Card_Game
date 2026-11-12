# Арты карт поля

Созданы встроенным ImageGen, без CLI/API. Пятнадцать новых изображений дополняют ранее готовую «Кузницу»: все 16 полей теперь имеют индивидуальный арт. Исходные PNG — 1122×1402, без рамки и текста; интерфейс добавляет их отдельно. Импорт через Unity MCP: Sprite Single, Full Rect, 100 PPU, sRGB, mipmaps, Clamp, Compressed HQ, max 2048, Read/Write выключен. Для новых непрозрачных изображений альфа отключена.

## Файлы

| Карта | ID | PNG |
|---|---|---|
| Тёплый очаг | `field.warm-hearth` | [warm-hearth-art-v1.png](../Assets/Art/Cards/Fields/warm-hearth-art-v1.png) |
| Тихий пруд | `field.quiet-pond` | [quiet-pond-art-v1.png](../Assets/Art/Cards/Fields/quiet-pond-art-v1.png) |
| Горная тропа | `field.mountain-path` | [mountain-path-art-v1.png](../Assets/Art/Cards/Fields/mountain-path-art-v1.png) |
| Степной простор | `field.steppe` | [steppe-art-v1.png](../Assets/Art/Cards/Fields/steppe-art-v1.png) |
| Лунная поляна | `field.moon-glade` | [moon-glade-art-v1.png](../Assets/Art/Cards/Fields/moon-glade-art-v1.png) |
| Старый алтарь | `field.old-altar` | [old-altar-art-v1.png](../Assets/Art/Cards/Fields/old-altar-art-v1.png) |
| Родовая крепость | `field.ancestral-fortress` | [ancestral-fortress-art-v1.png](../Assets/Art/Cards/Fields/ancestral-fortress-art-v1.png) |
| Обитель отшельников | `field.hermit-abode` | [hermit-abode-art-v1.png](../Assets/Art/Cards/Fields/hermit-abode-art-v1.png) |
| Бездна | `field.abyss` | [abyss-art-v1.png](../Assets/Art/Cards/Fields/abyss-art-v1.png) |
| Поле славы | `field.field-of-glory` | [field-of-glory-art-v1.png](../Assets/Art/Cards/Fields/field-of-glory-art-v1.png) |
| Зеркальное озеро | `field.mirror-lake` | [mirror-lake-art-v1.png](../Assets/Art/Cards/Fields/mirror-lake-art-v1.png) |
| Зал смирения | `field.hall-of-humility` | [hall-of-humility-art-v1.png](../Assets/Art/Cards/Fields/hall-of-humility-art-v1.png) |
| Вечный маяк | `field.eternal-beacon` | [eternal-beacon-art-v2.png](../Assets/Art/Cards/Fields/eternal-beacon-art-v2.png) |
| Жертвенный алтарь | `field.sacrificial-altar` | [sacrificial-altar-art-v1.png](../Assets/Art/Cards/Fields/sacrificial-altar-art-v1.png) |
| Хрустальный мост | `field.crystal-bridge` | [crystal-bridge-art-v1.png](../Assets/Art/Cards/Fields/crystal-bridge-art-v1.png) |

Кузница: [forge-art-v1.png](../Assets/Art/Cards/Fields/forge-art-v1.png). Все изображения подключены полем Art соответствующего FieldDef.

## Финальные запросы

### Тёплый очаг

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Тёплый очаг. A welcoming ancient stone cottage interior at dusk. A large round stone hearth with orange fire and glowing coals is the clear focal point center-right; chunky sandstone blocks, a simple wooden bench, earthen tiles, a small open arch toward a quiet mountain village, soft amber light against muted teal shadows. Cozy and grounded, fire and earth, no people. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Тихий пруд

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Тихий пруд. A small tranquil clear freshwater pond in a sheltered woodland clearing at golden morning. Smooth pale stones around the shore, a few water lilies, shafts of warm sunlight reflecting on turquoise water; one luminous smooth rock and gentle ripples form a focal point center-right. Intimate, peaceful water and sunlight, distinct from an alpine lake. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Горная тропа

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Горная тропа. A winding old stone path cut into huge ochre mountain cliffs, leading toward a sunlit saddle and a monumental rugged peak center-right. Strong ascending perspective, sparse tough grasses, windblown clouds and warm light on layered rocks. Earth and wind, adventurous but peaceful, bold readable landscape shapes. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Степной простор

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Степной простор. A vast open golden steppe under a dramatic sweeping sky. Long ochre grasses bending together in strong wind, rolling grassy hills, a solitary low warm sandstone outcrop center-right, amber late-afternoon sun and pale turquoise cloud shadows. The expansive wind-swept plain is the subject, no buildings or forests. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Лунная поляна

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Лунная поляна. A hidden woodland glade at blue-violet moonlit night. A luminous crescent moon above a small dark pool, silver-lit grass and broad ancient tree trunks framing the scene, smooth stones and subtle violet-blue firefly-like magic reflected in water. Focal pool and moon reflection center-right; calm mysterious darkness with legible layered silhouettes, not black. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Старый алтарь

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Старый алтарь. An old weathered pale stone altar in a quiet ruined sanctuary. Low cracked circular dais with a warm soft golden light above it center-right, broken stone arches, moss in joints, cool deep violet recesses and a few tiny white flowers. A place of mercy and refuge, sunlight meeting gentle shadow, no threatening ritual. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Родовая крепость

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Родовая крепость. A monumental ancestral sandstone fortress rising from a rocky mountain shelf at sunrise. Massive stepped walls, carved angular buttresses, a strong closed golden-lit gateway center-right, white-gold watchtowers and windswept teal clouds. Earthy ochre rock and warm ivory light, secure and dignified, no armies or heraldic writing. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Обитель отшельников

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Обитель отшельников. A secluded ancient hermit monastery on a narrow high rocky terrace. Small ivory stone buildings and one tall quiet arch center-right, a winding stone stair, dry mountain cliffs descending into deep violet mist, restrained warm sunlight on the sanctuary contrasting with cool shadows. Austere and intimate despite the immense landscape, no occupants. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Бездна

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Бездна. An immense flooded abyss between towering dark basalt cliffs. A swirling deep turquoise pool disappears into a violet magical chasm center-right, thin waterfalls descend from broken rock ledges, cold blue light catches wet stone, subtle purple radiance from below. Ominous but beautifully readable, deep layers and strong silhouettes, no monsters or bodies. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Поле славы

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Поле славы. An ancient open-air tournament field on a windswept high plateau. A broad worn circular stone arena, bronze victory pedestal center-right, a few plain burnt-orange pennants bending in the wind, distant jagged peaks and bright teal storm-cleared sky. Warm fiery sunlight and powerful cloud movement, heroic atmosphere; no people, weapons, trophies shaped like characters, or writing. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Зеркальное озеро

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Зеркальное озеро. A perfectly still alpine mirror lake beneath steep silver-blue mountains. A wide glasslike cyan water surface reflects the luminous sky and mountain peaks exactly, a sculptural pale stone shore curves in from lower right, a thin ribbon of mist and high airy clouds. Crystal blue and turquoise with quiet pale gold highlights, majestic open water rather than a small woodland pond. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Зал смирения

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Зал смирения. A vast sacred hall of pale stone with impossibly tall simple columns. A very small worn unadorned stone pedestal center-right is dwarfed by grand architecture, shallow clear water across the floor reflects columns, warm ivory sunbeams descend from a high opening into soft turquoise haze. Quiet, humble and awe-inspiring, light, earth and water; no throne, people or statues of characters. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Вечный маяк

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Вечный маяк. A legendary white-gold lighthouse on a monumental sea cliff, clearly placed center-right. Its brilliant warm magical beacon sends a broad luminous shaft through sweeping teal storm clouds; blue ocean waves crash far below, sunlit spray and windy cloud ribbons, dark rock arch leading toward the tower. Immovable, hopeful and eternal, rich contrast and bold readable silhouette. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Жертвенный алтарь

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Жертвенный алтарь. A legendary dark basalt altar atop a stepped volcanic sanctuary center-right. Angular obsidian blocks, glowing molten-orange fissures, one contained violet magical flame above a broad stone altar, huge shadowy arches and distant red-orange volcanic sky. Dramatic dark purple, ember orange and earthy bronze, solemn ritual atmosphere. Absolutely no people, victims, blood, gore, skulls or corpses. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

### Хрустальный мост

```text
Use case: stylized-concept. Asset type: standalone fantasy FIELD CARD environment illustration for an existing Unity card game. Match the established Forge art direction: polished stylized painterly fantasy, clearly painted angular stone forms, rich saturated but controlled colors, cinematic lighting, bold silhouettes readable on a small card; no photorealism and no flat vector look. Card subject: Хрустальный мост. A legendary translucent crystal bridge arching between floating jagged rock islands high above a deep misty sky chasm. The bridge curves from lower foreground toward center-right, faceted turquoise and violet crystal with subtle amber inner glow, delicate crystalline balustrades, immense airy cloud layers and a sunrise behind it. Dazzling but clean readable large shapes, magical wind and light, no people. Composition: portrait 4:5 full-bleed artwork. Place the main focal feature center-right and leave the leftmost 20% lower in detail because separate bonus medallions cover it. Keep important features away from extreme top corners, where an arched frame crops the art. This is ONLY the environment art, not the card layout. No borders, frames, plaques, UI, element icons, text, letters, numbers, watermarks, people or creatures. Opaque full background.
```

## Поправка композиции «Вечного маяка»

После просмотра в рамке фонарь оказался слишком высоко. Финальный v2 получен редактированием исходного арта тем же встроенным ImageGen; башня уменьшена, вокруг неё добавлено пространство. Исходный вариант остаётся в каталоге генераций, в проекте подключён только v2. Финальный запрос редактирования:

```text
Edit this environment illustration to improve its composition inside an arched FIELD CARD window. Keep the same painterly fantasy style, white-gold lighthouse, cyan storm clouds, warm golden beacon beam, rocky sea cliff, bridge, blue waves and hopeful mood. Pull the camera back substantially, making the whole lighthouse tower about 30% smaller, and place the glowing lantern centered at x=0.66, y=0.34 of the full portrait image (coordinates measured from top-left). Keep the complete tower from its golden tip to its base within y=0.27 through y=0.73. Add generous stormy sky above so the entire beacon remains visible even after the TOP 20% of this portrait is cropped off by the game's card framing. The lantern and origin of its beam MUST be comfortably below the upper quarter, not at the top edge. Keep the leftmost 20% quiet and shadowy; main landmark center-right. Full bleed portrait 4:5 environment only, no frame, card UI, borders, text, letters, numbers, people or creatures. Opaque background. Preserve attractive crisp painted detail and rich warm-cool lighting.
```
