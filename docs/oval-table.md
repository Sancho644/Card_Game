# Нефритовый игровой стол

Реализован по утверждённому макету 13 ноября 2026 года: нефритовая поверхность,
овальная золотая окантовка, бирюзовые вставки и наклонная перспективная камера.
В макете изображения и числа карт были условными; игра использует существующие карты и правила.

## Изменения

- Камера: наклон 82° → 55°, высота 18 → 19, начальный вертикальный FOV 36° → 35°.
  Сохранён существующий `CameraFramer`, который подбирает кадрирование по игровым объектам.
  Резерв под верхней плашкой уменьшен с 0.14 до 0.09.
- Расстояние рядов полей от центра: 1.95 → 2.2, чтобы разделить их при новом ракурсе.
  Поле по центру, своё существо справа, существо противника слева; размеры и правила не менялись.
- Материал `Table.mat` использует отдельную текстуру нефрита. Цвет поверхности
  уточнён после первого снимка: `(0.65, 0.82, 0.78)`.
- `OvalTable` в `Match.unity` создаёт двойной обод со скосами и вставками, тонкий
  разделитель сторон. Обод лежит в плоскости реального стола; размеры вычисляются
  по пересечениям лучей камеры с этой плоскостью и обновляются при смене кадра.
- Старая прямоугольная тонировка `TableSides` выключена. `TableSurfaceFitter`
  продолжает закрывать всю область камеры нефритовой поверхностью, сохраняя плотность текстуры.
- У декора нет коллайдеров: попадание по картам и игровому полю использует прежнюю систему ввода.
  Новые пакеты и постобработка не добавлялись, Core и ИИ не менялись.

## Ресурсы

- `Assets/Art/UI/BattleBoard/jade-v1.png` — материал поверхности, импорт с mipmaps и anisotropic 4.
- `Assets/Art/Materials/BoardGoldShadow.mat`, `BoardGoldFace.mat`, `BoardGoldHighlight.mat`,
  `BoardTurquoise.mat` — материалы обода и вставок, URP Unlit.
- `Assets/Data/Config/TableBoard.asset` — материалы, положение краёв в кадре,
  ширина и высота обода, количество сегментов, размер вставок и разделителя.
- `Assets/Data/Config/TablePresentation.asset` — параметры камеры.
- `Assets/Scripts/Presentation/Table/OvalTableView.cs` — геометрия и подгонка обода.

Обод и вставки: два MeshRenderer, 1337 вершин, 1958 треугольников.
Это бюджет геометрии, а не замер полного числа draw calls всей сцены.

## Проверка

Все 70 тестов `Game.Presentation.Tests.EditMode` прошли через Unity MCP.
В Play mode проверены 1920×1080 и 2100×900:

- Все четыре угла камеры закрыты поверхностью, чёрных боковых полос нет.
- Для рук из 1, 7 и 9 карт проверены 68 углов: карты в кадре.
- Лучи из центров всех трёх карт битвы попадают в правильные коллайдеры.
- Обратная проекция курсора в плоскость стола совпадает с зоной выставления поля.
- Геометрия конечна, без NaN/Infinity; после смены разрешения обод перестраивается.
- Кнопка открытия выдаёт допустимую команду; глаз исчезает, карты битвы сохраняют позиции.
- В консоли нет ошибок. Настройки Game View и фонового выполнения после проверки восстановлены.

Проверка выполнена в локальной партии с допустимыми командами. Это проверка отображения,
а не симуляция баланса. На физическом Android-устройстве проверка не проводилась.

Ручной проход: начать матч, выставить поле, перетащить существо, открыть поле в битве,
увеличить карту, проверить руку после возврата существ и переключить разрешение Game View.

## Оценка оформления после доработки

Palette discipline: 4/5 — нефрит, золото и бирюза соответствуют утверждённой палитре; ранее 3/5.
Style uniformity: 4/5 — каменная поверхность и окантовка сочетаются с рамками карт; ранее 3/5.
Lighting depth: 3/5 — перспектива и скосы дают глубину без новых экранных эффектов; ранее 2/5.
Silhouette: 3/5 — карты различимы в игровом масштабе; прежняя оценка 3/5.
UI intentionality: 4/5 — существующие оформленные HUD и окна сохранены; прежняя оценка 4/5.
Feedback visibility: 4/5 — подсветка поля, значки и открытие работают при новом ракурсе; прежняя оценка 4/5.

Применены рекомендации `art-direction.md`, `camera-rigs.md`, `scene-lighting.md` и
`visual-scorecard.md` навыка `unity:craft-game-floors`, с сохранением существующей камеры и ограничений проекта.
API сверены с документацией Unity: [ViewportPointToRay](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera.ViewportPointToRay.html),
[Mesh data](https://docs.unity3d.com/6000.0/Documentation/Manual/AnatomyofaMesh.html).

## Создание текстуры

Использован встроенный imagegen. Итоговый промпт:

> Production game environment material texture, square seamless tileable emerald jade tabletop ALBEDO. Rich deep emerald-green polished stone, subtle layered natural jade clouding, delicate thin irregular turquoise mineral veins with rare ivory-turquoise hairline highlights. Match an antique-gold-and-dark-jade hand-painted fantasy card game's luxurious emerald gaming table. Mostly calm dark emerald negative space so cards remain visually dominant, restrained saturation, no large white crack networks, no coarse broken slate, no lava or grass. All pattern is in the material itself, NOT emissive magical effects. Perfectly flat directly top-down orthographic texture scan, uniformly softly lit, no perspective, no horizon, no vignette, no cast shadows, no hotspots, no reflections of objects, no objects, no cards, no gold frame, no borders, no text, no symbols. Seamless on all four edges, fine detail suitable for a large tabletop. Hand-painted high quality realistic-stylized jade, smooth stone finish, not noisy photographic gravel.
