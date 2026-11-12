# Значки состояния карты поля

Реализован выбранный вариант 1 — закрытый глаз и замок.

- Закрытое поле владельца: глаз вместо надписи «закрыта».
- Запертое поле: замок заменяет глаз.
- Открытое поле и убранное поле: значок скрыт.
- В руке, коллекции и обычном увеличенном просмотре статического лица значков нет.
- Закрытое поле соперника сохраняет рубашку; его лицо не раскрывается.
- При анимации открытия значок снимается сразу, перед подсветкой/переворотом.
- Значок не затемняется вместе с лицом, не перехватывает нажатия и находится
  справа над названием, вне полосы бонусов и текста условия.

## Ресурсы и настройка

- `Assets/Art/Icons/field-closed-v1.png`
- `Assets/Art/Icons/field-locked-v1.png`

Ресурсы импортированы через Unity MCP как одиночные спрайты с прозрачностью,
без mipmaps, с максимальным размером 256×256. Исходные PNG сохранены целиком.
В `Assets/Data/Config/FieldCardStyle.asset` доступны Closed Icon, Locked Icon и
нормализованная область State Badge: `(0.69, 0.405, 0.25, 0.175)`.

Отображение получает закрытое/запертое состояние из `PlayerView` через `TableSync`.
Строки интерфейса не используются как признаки состояния. Core и правила не менялись.

## Создание графики

Использован встроенный imagegen; референс — первая пара утверждённого концепта.
Промпты для отдельных производственных спрайтов:

**Closed eye:**

> Production sprite extraction / refinement from the attached concept sheet. Return ONLY the CLOSED EYE medallion from the TOP LEFT of column 1, isolated, one centered icon filling about 86% of a square canvas. Match its approved design exactly: circular antique gold beveled rim, dark jade stone face, gold curved closed eyelid with three short lashes. Bold simple silhouette readable at 32 px. Entire circular medallion opaque, exterior genuinely transparent alpha with clean antialiased edges. No card, no text, no second icon, no sheet, no shadow outside the circle, no glow, no sparkles, no environmental background. The eye must be visibly CLOSED, not open, no diagonal slash. Preserve gold and jade painted fantasy material style.

**Padlock:**

> Production sprite extraction / refinement from the attached concept sheet. Return ONLY the PADLOCK medallion, second icon at the TOP of column 1, isolated, one centered icon filling about 86% of a square canvas. Match its approved design exactly: circular antique gold beveled rim, dark jade stone face, a sturdy GOLD CLOSED PADLOCK with a closed arched shackle, dark copper-red rectangular body inset and clearly visible small gold-rimmed keyhole. Bold simple silhouette readable at 32 px. Entire circular medallion opaque, exterior genuinely transparent alpha with clean antialiased edges. No card, no text, no second icon, no sheet, no shadow outside the circle, no glow, no sparkles, no environmental background. Preserve gold and jade painted fantasy material style, matching the closed-eye companion icon in the first column.

## Проверка

Через MCP прошли все 70 EditMode-тестов `Game.Presentation.Tests.EditMode`.
В Play mode проверены восемь пунктов: глаз после синхронизации состояния,
отсутствие прежней надписи, приоритет замка, сохранение яркости и состояния при
затемнении, скрытие в открытом состоянии, сохранение после повторной синхронизации,
отсутствие значков в руке и сохранение скрытой информации соперника.
Проверены снимки обоих состояний в сцене битвы на 1920×1080.

Ручной проход: выставить поле, проверить глаз; запретить открытие эффектом и проверить
замок; отдельно открыть доступное поле и проверить исчезновение значка во время анимации.
