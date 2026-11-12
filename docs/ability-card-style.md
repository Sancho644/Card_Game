# Карты способностей: стиль 2

Реализован выбранный концепт «Магический разлом»: большой арт в верхней половине, название под артом, компактная плашка прибавки к силе, момент розыгрыша, текст эффекта и подписи типа/редкости. Названия, числа, тексты и момент розыгрыша отрисовываются интерфейсом.

## Ассеты

- `Assets/Art/Cards/Abilities/ability-frame-v1.png` — общая рамка с прозрачным окном арта, без текста и плашки бонуса.
- `Assets/Art/Cards/Abilities/ability-bonus-v1.png` — отдельная плашка; у карт без собственного бонуса к силе скрывается.
- `Assets/Art/Cards/Abilities/intercept-art-v1.png` — первый готовый арт, подключён к `ability.intercept`.
- `Assets/Data/Config/AbilityCardStyle.asset` — графика, цвета, нормализованные области надписей, размер в мире и смещение лицевой стороны. Подключён к VisualSettings.

Шаблон применяется ко всем 34 способностям. Остальные 33 индивидуальных арта пока не готовы: вместо иллюстрации — тёмная область и значок редкости. Для добавления арта достаточно заполнить Art у AbilityDef.

## Данные и отображение

`AbilityFace` хранит бонус отдельно от базовой силы существа: Power у способности остаётся null. CardFaceBuilder берёт число из PowerBonusData, а подпись момента розыгрыша — из AbilityTiming через UiTexts. Для единственного числового бонуса убирается повторяющийся префикс вида «+40.» или «+50,» в отображаемом тексте; условия сохраняются. Полное исходное описание остаётся в CardFace.Body. Сложные предложения и штрафы сопернику сохраняются целиком. При нескольких отдельных прибавках бонусная плашка не агрегирует их, полный текст остаётся источником отображения.

`AbilityCardFaceView` используется в коллекции, руке, анимации розыгрыша и крупном просмотре битвы. Рубашка и скрытые карты не раскрывают тип, момент розыгрыша и бонус. На 3D-карте сохраняется непрозрачная подложка для корректного нахлёста. Новые подписи находятся в UiTexts. Core и правила не изменены.

## Проверка

- 290 EditMode-тестов: разделение бонуса и базовой силы, сохранение условий и штрафов, отсутствие данных у скрытой карты, размещение длинного текста на картах высотой 160 и 800.
- Play Mode: все 34 способности в коллекции, без обрезки текста, минимальный размер основного текста в крупном просмотре 17,95.
- Все 34 3D-карты: бонусная плашка по данным, затемнение, непрозрачная подложка, скрытие неизвестной карты и переключения на поле/существо.
- Крупный просмотр битвы: все способности и переключения между типами карт; проверка при 1280×720 и 2100×900.
- Консоль Unity без ошибок и предупреждений.

Проверить вручную: Коллекция → Способности → «Перехват» (арт и +40), «Благословение поля» (условие сохранено), «Отступление» (длинный текст и отсутствие плашки). В битве увеличить карту, закрыть просмотр и проверить нахлёст в руке.

## Финальные запросы ImageGen

Использован встроенный ImageGen, без CLI/API. Рамка получена редактированием выбранного макета; арт и бонусная плашка созданы отдельными вызовами. Изображения импортированы через Unity MCP как Sprite Single с mipmaps и Compressed HQ; альфа сохранена для рамки и плашки.

### Рамка

```text
Edit this APPROVED ability-card concept into a reusable EMPTY FRAME sprite for the game. Preserve the thin angular antique bronze-gold outer border, dark teal stone corner pieces, wide dark brown name plaque, quiet dark teal effect tablet and the two footer plaques. Remove ALL text, letters, numbers, bonus +40 plaque, rarity gemstone, magic shield ring, lightning, clouds and the entire illustration. The large upper ART WINDOW must be genuinely TRANSPARENT; there must be no shield ring or residual spell energy because different cards use different art. The name plaque and lower effect tablet remain opaque and BLANK. No separate header or panel for the timing: leave the rules tablet uninterrupted so the game draws the timing and description. Remove the separate bonus plaque completely: it is added as another sprite only on cards with a bonus. Portrait 7:10, straight flat front view, outer border fills canvas edges with no margins. Approximate zones from TOP: art window 2%-52%, brown name plaque 53%-62%, dark teal rules tablet 63%-92%, footer plaques 93%-99%. Footer plaques should both have neutral dark teal interiors (not purple); the game adds the rarity color/text separately. Leave top-right area able to receive a small rarity gem. Transparent outside clipped corners. Crisp painterly beveled metal texture matching the approved design, no UI text, no watermark.
```

### Перехват

```text
Use case: stylized-concept. Paint ONLY a full-bleed fantasy spell environment/action illustration for a card named Перехват, in the polished stylized painterly fantasy art direction of a bronze-and-teal card game. A vivid curved translucent CYAN magical shield inside an elliptical antique bronze energy ring intercepts an incoming VIOLET lightning spell. The impact near the middle sends purple crystalline fragments upward and to the right, with cyan energy ripples across the barrier. Moody deep blue and teal storm-cloud backdrop, strong bright impact and clean large shapes readable on a small card, beautifully painted rather than photographic. The ring is part of THIS illustration, not the card frame. Landscape 4:3 composition, complete ellipse comfortably inside the image, not cut off, main impact center, rich warm bronze against cold cyan/violet. ONLY art: no card layout, frame, plaques, gems, text, letters, numbers, UI, watermark, people or creatures. Opaque background.
```

### Плашка бонуса

```text
Use case: stylized-concept. Create ONE empty horizontal bonus plaque sprite for a fantasy card game. Match an antique bronze-gold thin beveled frame and dark warm brown stone interior. Long compact hexagonal shape with pointed left and right ends, width about 3.5 times height, front-facing completely flat, crisp painterly metal edge and restrained carved corners. It will contain a +40 bonus and a short label drawn separately by the game, so the entire center must be QUIET, DARK and BLANK. No text, numbers, symbols, gems, lightning, shadow or background. Genuine transparent pixels outside the plaque, opaque dark brown inside. Plaque fills canvas with minimal margins.
```
