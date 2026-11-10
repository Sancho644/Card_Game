# Рамки существ по стихиям

Пять вариантов созданы встроенным imagegen редактированием земной рамки; названия, сила и иконки остались отдельными слоями. Геометрия и прозрачное окно общие с исходным шаблоном. Изображения сохранены в `Assets/Art/Cards/Frames/`.

| Стихия | Файл | Оформление |
|---|---|---|
| Земля | creature-frame-v1.png | Исходный камень, бронза, тёмная бирюза |
| Огонь | creature-frame-fire-v1.png | Уголь, медь, тлеющие швы |
| Вода | creature-frame-water-v1.png | Синий лак, серебро, волны |
| Ветер | creature-frame-wind-v1.png | Зелёная эмаль, серебро, плавные линии |
| Свет | creature-frame-light-v1.png | Слоновая кость, шампанское золото |
| Тьма | creature-frame-dark-v1.png | Обсидиан, аметист, серебро |

Назначения хранятся в `VisualSettings → Elements → CreatureFrame`. `CardFaceBuilder` выбирает рамку по стихии существа, а общий `CreatureCardFaceView` показывает её в коллекции, руке, на столе и в крупном просмотре. При неназначенной рамке используется базовая рамка CreatureCardStyle. Текстовые панели всех вариантов остаются тёмными для кремовых надписей.

## Промпты

Исходное изображение: `Assets/Art/Cards/Frames/creature-frame-v1.png`. Для каждого вызова включён прозрачный фон; конкретный API ID модели не задавался.

### Огонь

```text
Edit this existing reusable fantasy creature-card FRAME OVERLAY to create another ELEMENTAL MATERIAL VARIANT in the EXACT SAME game style. Preserve the EXACT portrait silhouette, geometry, outer perimeter, top-left circular power badge, top-right circular element badge with small rectangular label, wide lower title plaque and two small footer plaques. Keep their sizes and locations identical; they must fit the same existing text layout. Retain the same restrained, high-quality stylized painted fantasy craftsmanship and beveled metal edges. ONLY change the elemental color palette, materials and very subtle decoration INSIDE existing frame surfaces. ALL badge interiors and nameplates must stay DARK enough for cream-colored text. No letters, no numbers, no symbols, no element icons, no character, no landscape, no added badges. Main central art aperture and everything outside the card silhouette must have TRUE ALPHA TRANSPARENCY. Do not fill transparency with a flat color, mist, smoke, water or checkerboard. Do not put effects across the art aperture or enlarge the perimeter. Avoid anything resembling a different card tier or extravagant jewels; these are all common-card base frames. Fire element: charcoal-black volcanic glass and dark burnt umber panels, warmly burnished copper and red-bronze edging, small restrained ember-orange seams and subtle flame-shaped etched corner details. Clearly fiery warm red/copper identity, not the original earth/teal palette. Dark brown-black empty power and element badge faces.
```

### Вода

```text
Edit this existing reusable fantasy creature-card FRAME OVERLAY to create another ELEMENTAL MATERIAL VARIANT in the EXACT SAME game style. Preserve the EXACT portrait silhouette, geometry, outer perimeter, top-left circular power badge, top-right circular element badge with small rectangular label, wide lower title plaque and two small footer plaques. Keep their sizes and locations identical; they must fit the same existing text layout. Retain the same restrained, high-quality stylized painted fantasy craftsmanship and beveled metal edges. ONLY change the elemental color palette, materials and very subtle decoration INSIDE existing frame surfaces. ALL badge interiors and nameplates must stay DARK enough for cream-colored text. No letters, no numbers, no symbols, no element icons, no character, no landscape, no added badges. Main central art aperture and everything outside the card silhouette must have TRUE ALPHA TRANSPARENCY. Do not fill transparency with a flat color, mist, smoke, water or checkerboard. Do not put effects across the art aperture or enlarge the perimeter. Avoid anything resembling a different card tier or extravagant jewels; these are all common-card base frames. Water element: deep navy and ocean-blue lacquered stone panels, brushed silver and cool cyan edging, subtle engraved wave lines inside the corner bevels, clean polished aqueous highlights. Clear blue/silver identity. Dark navy empty badge and nameplate interiors, no actual water across the opening.
```

### Ветер

```text
Edit this existing reusable fantasy creature-card FRAME OVERLAY to create another ELEMENTAL MATERIAL VARIANT in the EXACT SAME game style. Preserve the EXACT portrait silhouette, geometry, outer perimeter, top-left circular power badge, top-right circular element badge with small rectangular label, wide lower title plaque and two small footer plaques. Keep their sizes and locations identical; they must fit the same existing text layout. Retain the same restrained, high-quality stylized painted fantasy craftsmanship and beveled metal edges. ONLY change the elemental color palette, materials and very subtle decoration INSIDE existing frame surfaces. ALL badge interiors and nameplates must stay DARK enough for cream-colored text. No letters, no numbers, no symbols, no element icons, no character, no landscape, no added badges. Main central art aperture and everything outside the card silhouette must have TRUE ALPHA TRANSPARENCY. Do not fill transparency with a flat color, mist, smoke, water or checkerboard. Do not put effects across the art aperture or enlarge the perimeter. Avoid anything resembling a different card tier or extravagant jewels; these are all common-card base frames. Wind element: dark forest-green and desaturated jade enamel panels, pale brushed silver with soft mint accents, a few elegant curved wind-groove engravings contained inside the existing corner shapes. Light agile mint-green/silver identity; no feathers or protrusions, no clouds across the opening. Empty dark green-black badge and nameplate interiors.
```

### Свет

```text
Edit this existing reusable fantasy creature-card FRAME OVERLAY to create another ELEMENTAL MATERIAL VARIANT in the EXACT SAME game style. Preserve the EXACT portrait silhouette, geometry, outer perimeter, top-left circular power badge, top-right circular element badge with small rectangular label, wide lower title plaque and two small footer plaques. Keep their sizes and locations identical; they must fit the same existing text layout. Retain the same restrained, high-quality stylized painted fantasy craftsmanship and beveled metal edges. ONLY change the elemental color palette, materials and very subtle decoration INSIDE existing frame surfaces. ALL badge interiors and nameplates must stay DARK enough for cream-colored text. No letters, no numbers, no symbols, no element icons, no character, no landscape, no added badges. Main central art aperture and everything outside the card silhouette must have TRUE ALPHA TRANSPARENCY. Do not fill transparency with a flat color, mist, smoke, water or checkerboard. Do not put effects across the art aperture or enlarge the perimeter. Avoid anything resembling a different card tier or extravagant jewels; these are all common-card base frames. Light element: softly luminous ivory ceramic perimeter inlays and restrained champagne-gold edges with pale warm-yellow accents, subtle straight ray engravings contained in the corners. Bright ivory/gold external frame identity, but ALL empty badge and nameplate text surfaces must remain deep warm charcoal-brown for strong cream-text contrast. No glowing fog or star symbols, no effects in aperture.
```

### Тьма

```text
Edit this existing reusable fantasy creature-card FRAME OVERLAY to create another ELEMENTAL MATERIAL VARIANT in the EXACT SAME game style. Preserve the EXACT portrait silhouette, geometry, outer perimeter, top-left circular power badge, top-right circular element badge with small rectangular label, wide lower title plaque and two small footer plaques. Keep their sizes and locations identical; they must fit the same existing text layout. Retain the same restrained, high-quality stylized painted fantasy craftsmanship and beveled metal edges. ONLY change the elemental color palette, materials and very subtle decoration INSIDE existing frame surfaces. ALL badge interiors and nameplates must stay DARK enough for cream-colored text. No letters, no numbers, no symbols, no element icons, no character, no landscape, no added badges. Main central art aperture and everything outside the card silhouette must have TRUE ALPHA TRANSPARENCY. Do not fill transparency with a flat color, mist, smoke, water or checkerboard. Do not put effects across the art aperture or enlarge the perimeter. Avoid anything resembling a different card tier or extravagant jewels; these are all common-card base frames. Darkness element: deep obsidian and midnight-purple enamel panels, muted silver and amethyst-violet bevels, restrained angular shadow-like engraved corners, cool polished edge reflections. Distinct dark violet/black/silver identity; no skulls, no eyes, no tentacles, no protrusions, no purple fog across aperture. Dark near-black purple badge and nameplate interiors.
```

## Проверка

261/261 EditMode: шесть различных рамок назначены, все существа получают рамку своей стихии, отображаемый Sprite соответствует данным. В Play mode просмотрены шесть обычных существ разных стихий одновременно в коллекции. Консоль без ошибок и предупреждений.
