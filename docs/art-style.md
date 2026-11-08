# Стиль артов карт

Автор выбрал вариант №2 «Каменного жука»: яркое стилизованное мультяшное фэнтези.

Эталон: `Assets/Art/Cards/Creatures/creature.stone-beetle-v1.png`. Арт назначен в поле `_art` карты `creature.stone-beetle`; рамка и игровые надписи формируются Unity.

## Ориентиры

- Крупные, ясные формы и выразительный силуэт, узнаваемый в маленьком размере.
- Стилизованные материалы: несколько крупных каменных пластин вместо мелкого визуального шума.
- Живые, привлекательные персонажи; выражение и пропорции определяются характером существа.
- Живописная двухмерная иллюстрация с чистыми тенями и мягкими бликами.
- Цвета стихии читаются в персонаже; фон проще и слабее по контрасту.
- Персонаж занимает большую часть кадра. Важные детали не прижимать к краям, учитывать обрезку в окне арта.
- В арте нет названий, чисел силы, рамок и значков интерфейса.
- Для следующих артов прикладывать эталон №2 как референс стиля, но не копировать анатомию жука другим существам.

## Генерация эталона

Создан встроенным генератором изображений (imagegen). Конкретный API ID модели инструментом не задавался. Исходный файл сохранён без изменений, импортирован в Unity как Sprite; отклонённые варианты не импортированы.

Использованный промпт:

```text
Use case: stylized-concept. Asset type: standalone illustration for the artwork window of an original fantasy collectible card game. Subject: 'Stone Beetle', an Earth-element common creature. One sturdy six-legged beetle, recognizable insect anatomy, chunky layered sandstone shell like natural rock plates, modest beetle mandibles, small amber eyes, a few tufts of moss in its armor joints. Original creature, no franchise references. Scene: a mossy rocky woodland floor, muted distant fern silhouettes. Composition: a clean readable three-quarter view of the entire beetle, facing slightly left, character occupying about 70% of the image, all six legs clearly attached, simple background with strong subject separation, coherent perspective. Earthy ochre, slate, moss green, restrained amber accents. Match this exact subject, scene and framing across the intended style exploration. No card border, no UI, no letters, no numbers, no caption, no watermark. STYLE B: colorful stylized animated fantasy game illustration, appealing chunky rounded proportions, crisp confident shapes, clean cel-painted shading, expressive friendly beetle face, broad soft highlights. Cheerful adventurous mood, rich but controlled colors, beautifully designed large rock plates; high clarity and readable silhouette. 2D painted animation key art, not a plastic 3D render.
```
