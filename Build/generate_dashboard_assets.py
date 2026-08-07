from pathlib import Path
import argparse
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageEnhance

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets" / "UI"
parser = argparse.ArgumentParser(description="Generate a resolution-specific Project Liberty dashboard texture")
parser.add_argument("--height", type=int, default=2160, choices=(720, 1080, 1440, 2160))
args = parser.parse_args()
H = args.height
W = H * 16 // 9
S = H / 720.0

PINK = (235, 54, 119, 255)
WHITE = (244, 246, 248, 255)
MUTED = (154, 159, 165, 255)
BORDER = (74, 81, 86, 150)
PANEL = (9, 14, 17, 224)
CARD = (19, 24, 27, 238)


def px(value):
    return int(round(value * S))


def font(size, bold=False):
    name = "arialbd.ttf" if bold else "arial.ttf"
    return ImageFont.truetype(str(Path("C:/Windows/Fonts") / name), px(size))


background = Image.open(ASSETS / "dashboard-background.png").convert("RGB")
background = background.resize((W, H), Image.Resampling.LANCZOS).filter(ImageFilter.GaussianBlur(0.7))
background = ImageEnhance.Brightness(background).enhance(1.55)
background = ImageEnhance.Contrast(background).enhance(1.08).convert("RGBA")
overlay = Image.new("RGBA", (W, H), (0, 6, 9, 48))
image = Image.alpha_composite(background, overlay)
draw = ImageDraw.Draw(image, "RGBA")


def rect(x, y, width, height, fill, radius=0, outline=None, stroke=1):
    box = (px(x), px(y), px(x + width), px(y + height))
    draw.rounded_rectangle(box, radius=px(radius), fill=fill, outline=outline, width=px(stroke))


def text(value, x, y, size, fill=WHITE, bold=False, anchor="la"):
    draw.text((px(x), px(y)), value, font=font(size, bold), fill=fill, anchor=anchor)


def panel(x, y, width, height, fill=PANEL, radius=7):
    rect(x, y, width, height, fill, radius, BORDER, 1)


def info_row(icon, label, value, x, y):
    text(icon, x, y, 11, MUTED)
    text(label, x + 17, y, 10, MUTED)
    text(value, x + 100, y, 10, WHITE)


def action_card(x, y, icon, title, subtitle, trailing=">"):
    panel(x, y, 263, 47, CARD, 5)
    text(icon, x + 13, y + 12, 17, PINK, True)
    text(title, x + 42, y + 10, 10, WHITE, True)
    text(subtitle, x + 42, y + 27, 8, MUTED)
    if trailing:
        text(trailing, x + 246, y + 17, 12, WHITE, False, "ma")


def stat_card(x, icon, label, value, note, progress=False):
    panel(x, 441, 108, 108, CARD, 5)
    text(icon, x + 13, 454, 16, PINK, True)
    text(label, x + 13, 481, 8, MUTED)
    text(value, x + 13, 501, 18, WHITE, True)
    text(note, x + 13, 530, 7, MUTED)
    if progress:
        rect(x + 13, 540, 70, 4, (65, 70, 74, 255), 2)
        rect(x + 13, 540, 40, 4, PINK, 2)


def achievement(y, title, subtitle, progress, done=False):
    panel(923, y, 298, 47, CARD, 5)
    rect(933, y + 11, 25, 25, (73, 25, 44, 230), 12)
    text("V" if done else "A", 945, y + 23, 10, PINK, True, "mm")
    text(title, 966, y + 8, 10, WHITE, True)
    text(subtitle, 966, y + 26, 8, MUTED)
    text(progress, 1205, y + 23, 9, PINK if done else WHITE, False, "rm")


# Sidebar and main glass frame
rect(0, 0, 258, 720, (3, 9, 13, 246))
rect(258, 0, 1022, 720, (4, 9, 12, 35))
panel(258, 15, 1000, 686, (12, 17, 20, 28), 9)

# Logo
logo = Image.open(ASSETS / "project-liberty-logo.png").convert("RGBA")
logo.thumbnail((px(55), px(62)), Image.Resampling.LANCZOS)
logo_layer = Image.new("RGBA", image.size)
logo_layer.alpha_composite(logo, (px(25) + (px(55) - logo.width) // 2, px(18) + (px(62) - logo.height) // 2))
image = Image.alpha_composite(image, logo_layer)
draw = ImageDraw.Draw(image, "RGBA")
text("P R O J E C T", 84, 30, 18, WHITE, True)
text("L I B E R T Y", 84, 53, 18, PINK, True)
text("Hard RP Mod", 84, 82, 10, MUTED)

nav = ["Главная", "Персонаж", "Статистика", "Настройки мода", "Управление отрядом", "Достижения", "Журнал событий", "Помощь", "Выход из мода"]
nav_icons = ["H", "P", "S", "N", "U", "A", "J", "?", "X"]
for index, label in enumerate(nav):
    y = 122 + index * 40
    if index == 0:
        rect(19, y, 223, 38, (65, 20, 39, 150), 4)
        rect(19, y, 3, 38, PINK, 2)
    color = WHITE if index == 0 else MUTED
    text(nav_icons[index], 37, y + 12, 12, PINK if index == 0 else MUTED, True)
    text(label, 63, y + 12, 11, color)
text("PROJECT LIBERTY v1.0.0", 35, 642, 9, MUTED)
text("Nothing Is Scripted", 35, 660, 10, PINK)
rect(35, 681, 150, 1, PINK)

# Header
text("ГЛАВНАЯ", 279, 40, 16, WHITE, True)
text("Добро пожаловать в Project Liberty", 279, 65, 11, MUTED)
panel(883, 36, 232, 45, (18, 23, 26, 185), 5)
text("O", 901, 48, 15, PINK, True)
text("ОФИЦЕР", 930, 43, 10, WHITE, True)
text("LSPD", 930, 61, 8, MUTED)
text("#", 1015, 48, 14, WHITE)
text("10-8", 1041, 43, 10, WHITE, True)
text("Доступен", 1041, 61, 8, MUTED)
panel(1131, 36, 106, 45, (12, 17, 20, 190), 5)
text("O", 1147, 48, 12, WHITE)
text("10:24", 1174, 43, 10, WHITE, True)
text("03.06.2024", 1174, 61, 7, MUTED)

# Character card
panel(277, 98, 385, 285)
text("МОЙ ПЕРСОНАЖ", 294, 113, 12, WHITE, True)
panel(293, 140, 136, 224, (12, 17, 20, 205), 5)
officer = Image.open(ASSETS / "officer.png").convert("RGBA")
bbox = officer.getbbox()
officer = officer.crop(bbox)
target_h = px(235)
target_w = int(officer.width * target_h / officer.height)
officer = officer.resize((target_w, target_h), Image.Resampling.LANCZOS)
character_layer = Image.new("RGBA", image.size)
character_layer.alpha_composite(officer, (px(295 + (132 - target_w / S) / 2), px(135)))
image = Image.alpha_composite(image, character_layer)
draw = ImageDraw.Draw(image, "RGBA")
text("John Roman", 451, 143, 16, WHITE, True)
text("Офицер полиции I", 451, 169, 10, PINK)
info_row("#", "Бейдж", "#1247", 451, 195)
info_row("D", "Отдел", "Патрульный отдел", 451, 218)
info_row("R", "Ранг", "Офицер полиции I", 451, 241)
info_row("C", "Позывной", "2-ADAM-12", 451, 264)
info_row("T", "Время в службе", "25 ч. 42 мин.", 451, 287)
rect(451, 330, 194, 32, (22, 25, 28, 255), 5, PINK, 1)
text("ПЕРЕЙТИ К ПЕРСОНАЖУ     >", 548, 346, 9, PINK, True, "mm")

# Quick access
panel(676, 98, 295, 285)
text("БЫСТРЫЙ ДОСТУП", 692, 113, 12, WHITE, True)
action_card(692, 144, "+", "ВЗЯТЬ СНАРЯЖЕНИЕ", "Оружие, броня и экипировка")
action_card(692, 199, "V", "СЛУЖЕБНЫЙ ТРАНСПОРТ", "Выбрать служебный автомобиль")
action_card(692, 256, "!", "ПОСЛЕДНИЙ ВЫЗОВ", "10-31: Кража автомобиля", "0.42 mi")
action_card(692, 321, "#", "КОДЕКС И ПРАВИЛА", "Законы штата и департамента", None)

# Notifications
panel(988, 98, 249, 285)
text("УВЕДОМЛЕНИЯ", 1004, 113, 12, WHITE, True)
notices = [
    ((255, 65, 55, 255), "Новое обновление мода", "Доступна версия 1.0.1", "Вчера"),
    ((246, 213, 70, 255), "Ежедневное задание", "Патрулируйте 60 минут", "10:00"),
    ((246, 213, 70, 255), "Тренировка в тире", "Доступна новая тренировка", "09:15"),
    ((78, 224, 159, 255), "Отчет готов", "Ваш отчет за смену готов", "Вчера"),
]
for i, (color, title, subtitle, when) in enumerate(notices):
    y = 145 + i * 46
    rect(1004, y, 217, 1, (90, 95, 99, 70))
    text("•", 1011, y + 16, 17, color, True, "mm")
    text(title, 1027, y + 9, 9, WHITE, True)
    text(subtitle, 1027, y + 26, 7, MUTED)
    text(when, 1208, y + 9, 7, MUTED, False, "ra")
panel(1004, 337, 217, 31, CARD, 5)
text("ВСЕ УВЕДОМЛЕНИЯ", 1110, 353, 9, PINK, True, "mm")
rect(1200, 346, 12, 14, PINK, 2)
text("4", 1206, 353, 7, WHITE, True, "mm")

# Stats
panel(277, 400, 614, 165)
text("СТАТИСТИКА ПЕРСОНАЖА", 294, 416, 12, WHITE, True)
stat_card(293, "T", "Время в игре", "25ч. 42м.", "Общее время")
stat_card(413, "C", "Принято вызовов", "38", "В этом месяце")
stat_card(533, "A", "Арестов", "27", "В этом месяце")
stat_card(653, "F", "Выписано штрафов", "56", "В этом месяце")
stat_card(773, "L", "Уровень", "12", "1240 / 2500 XP", True)

# Achievements
panel(907, 400, 330, 285)
text("ДОСТИЖЕНИЯ", 923, 416, 12, WHITE, True)
text("СМОТРЕТЬ ВСЕ", 1219, 418, 8, PINK, True, "ra")
achievement(444, "Первый день на службе", "Отслужите 1 час", "Выполнено", True)
achievement(499, "Закон и порядок", "Арестуйте 10 человек", "7 / 10")
achievement(554, "Профессионал", "Примите 50 вызовов", "38 / 50")
achievement(609, "Меткий стрелок", "Попадите в цель 100 раз в тире", "62 / 100")

# Characters
panel(277, 579, 614, 106)
text("ПЕРСОНАЖИ", 294, 591, 11, WHITE, True)
panel(293, 609, 184, 67, (38, 19, 29, 235), 5)
draw.rounded_rectangle((px(293), px(609), px(477), px(676)), radius=px(5), outline=PINK, width=px(1))
rect(305, 622, 34, 41, (29, 34, 37, 255), 3)
text("P", 322, 643, 14, PINK, True, "mm")
text("John Roman", 349, 620, 10, WHITE, True)
text("Офицер полиции I", 349, 638, 8, MUTED)
text("LSPD", 349, 655, 8, MUTED)
text("АКТИВЕН", 465, 658, 7, (125, 220, 133, 255), True, "ra")
panel(488, 609, 177, 67, CARD, 5)
rect(500, 622, 34, 41, (29, 34, 37, 255), 3)
text("P", 517, 643, 14, MUTED, True, "mm")
text("Michael Carter", 544, 620, 10, WHITE, True)
text("Детектив", 544, 638, 8, MUTED)
text("LSPD", 544, 655, 8, MUTED)
text("ВЫБРАТЬ", 653, 658, 7, WHITE, True, "ra")
panel(677, 609, 202, 67, CARD, 5)
text("＋", 704, 642, 22, PINK, False, "mm")
text("Создать персонажа", 736, 620, 10, WHITE, True)
text("Начать карьеру в полиции", 736, 641, 8, MUTED)
text("СОЗДАТЬ", 866, 658, 7, WHITE, True, "ra")

# Flatten every translucent UI surface against the city scene. CustomSprite otherwise
# blends low-alpha panel pixels against the game frame instead of the intended background.
image = Image.alpha_composite(background, image)
output_path = ASSETS / f"dashboard-shell-{W}x{H}.png"
image.convert("RGB").save(output_path, optimize=True)
if H == 2160:
    image.convert("RGB").save(ASSETS / "dashboard-shell.png", optimize=True)

# White tintable hover pixel and explicit mouse cursor.
Image.new("RGBA", (4, 4), (255, 255, 255, 255)).save(ASSETS / "white-pixel.png")
cursor = Image.new("RGBA", (40, 56), (0, 0, 0, 0))
cd = ImageDraw.Draw(cursor)
points = [(2, 2), (2, 43), (12, 34), (19, 52), (27, 48), (19, 31), (34, 31)]
cd.polygon(points, fill=(250, 250, 252, 255), outline=(5, 8, 10, 255))
cursor.save(ASSETS / "cursor.png")

print(f"Generated {output_path}")
