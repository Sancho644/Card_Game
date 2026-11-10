# Игровой шаблон карты существа

10 ноября 2026: утверждённый вариант №2 применён к существам в руке и на столе, в крупном просмотре и в коллекции. Сила сверху слева, стихия сверху справа, название снизу. Тип и редкость под названием. Для существ с описанием над названием отображается отдельная область текста. Карты поля и способности используют прежнее оформление.

## Ассеты и настройка

- `Assets/Art/Cards/Frames/creature-frame-v1.png` — общая рамка с прозрачным окном, без встроенных надписей, числа силы и символа стихии.
- `Assets/Data/Config/CreatureCardStyle.asset` — рамка, цвет текста, нормализованные прямоугольники элементов, размер игровой карты и глубина её лица.
- `Assets/Data/Config/VisualSettings.asset` ссылается на новый стиль.
- `CreatureCardFaceView` собирает арт, рамку, иконку и текст отдельными слоями. Один компонент используется коллекцией, CardView и CardZoomView. Арт заполняет окно с центральной обрезкой без растяжения.
- Единственный готовый арт существа пока — Каменный жук. Остальные существа показывают цвет и значок своей стихии внутри нового шаблона.

Сила берётся из CardFace и меняется через прежние SetPower/CountPower/FlashPower. PowerAnchor перенесён в верхний левый значок для всплывающих изменений. Рубашка, выделение, перетаскивание, полёт и увеличение карты остаются связанными с исходным CardView.

## Проверка

259/259 EditMode. Новые тесты проверяют обновление силы, возврат к базовой силе, затемнение без сброса числа, скрытие лица при привязке закрытой карты и переход к оформлению способности. В Play mode визуально проверены коллекция, рука и крупный просмотр Каменного жука; консоль без ошибок.

Ручная проверка: запустить Bootstrap, посмотреть существо в коллекции, начать матч и открыть крупный просмотр; в битве проверить плавное изменение силы, цветовую вспышку и всплывающие надписи у нового значка. Проверить длинное имя легендарного существа и рубашки противника.

## Получение рамки

Рамка создана встроенным imagegen редактированием утверждённого макета `docs/art-references/creature-card-layout-v1.png`, с запросом прозрачного фона; PNG импортирован как Sprite. Конкретный API ID модели не задавался.

Промпт:

```text
Edit the provided approved fantasy creature card into a production UI FRAME OVERLAY. Preserve EXACTLY the portrait dimensions, proportions, dark teal carved stone and restrained bronze perimeter, top-left circular dark power badge, top-right circular element badge with small rectangular nameplate, broad dark title plaque near bottom, and two smaller bottom type/rarity plates. Keep all badge surfaces OPAQUE dark stone, EMPTY and clean. REMOVE ALL letters, numbers, typography and symbols from EVERY plaque: remove 135, the Earth stones symbol, Земля, Каменный жук, Существо, Обычная. DO NOT leave imprinted marks or ghost letters. REMOVE the beetle and the ENTIRE forest illustration, replacing the central art area with TRUE ALPHA TRANSPARENCY from just inside the top perimeter down to the top edge of the lower large title plaque, excluding the two upper opaque badge shapes, which must stay intact over the transparent hole. The background outside the card silhouette must also be truly transparent, not black, not gray, not checkerboard. Only the EMPTY frame, empty badge backgrounds, lower nameplate and teal lower footer are visible. No creature, no landscape, no added icons, no lettering, no watermark. A clean reusable game UI overlay suitable to layer over ANY artwork, matching the supplied approved card closely.
```

Итог Play mode через MCP: партия против ИИ завершилась WinsReached, 4 битвы и 2 общие выкладки полей; ошибок и предупреждений Unity нет.
