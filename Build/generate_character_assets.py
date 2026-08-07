from pathlib import Path
from PIL import Image, ImageDraw, ImageEnhance, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets" / "UI"
RESOLUTIONS = ((1280, 720), (1920, 1080), (2560, 1440), (3840, 2160))

PINK = (235, 54, 119, 255)
WHITE = (245, 247, 249, 255)
MUTED = (154, 160, 167, 255)
PANEL = (5, 10, 14, 238)
CARD = (15, 20, 25, 238)
BORDER = (73, 80, 88, 155)


def make_font(scale, size, bold=False):
    name = "arialbd.ttf" if bold else "arial.ttf"
    return ImageFont.truetype(str(Path("C:/Windows/Fonts") / name), round(size * scale))


def draw_logo(canvas, logo, scale, x, y, width, height):
    target = logo.copy()
    target.thumbnail((round(width * scale), round(height * scale)), Image.Resampling.LANCZOS)
    canvas.alpha_composite(target, (round((x + width / 2) * scale - target.width / 2),
                                    round((y + height / 2) * scale - target.height / 2)))


def helpers(image, scale):
    draw = ImageDraw.Draw(image, "RGBA")

    def p(value): return round(value * scale)

    def rect(x, y, w, h, fill, radius=0, outline=None, stroke=1):
        draw.rounded_rectangle((p(x), p(y), p(x+w), p(y+h)), radius=p(radius),
                               fill=fill, outline=outline, width=max(1, p(stroke)))

    def text(value, x, y, size, fill=WHITE, bold=False, anchor="la"):
        draw.text((p(x), p(y)), value, font=make_font(scale, size, bold), fill=fill, anchor=anchor)

    return rect, text


def sidebar(image, logo, scale, active):
    rect, text = helpers(image, scale)
    rect(0, 0, 258, 720, PANEL)
    draw_logo(image, logo, scale, 25, 18, 55, 62)
    text("P R O J E C T", 85, 28, 17, WHITE, True)
    text("L I B E R T Y", 85, 51, 17, PINK, True)
    text("Hard RP Mod", 85, 78, 9, MUTED)
    labels = ["Главная", "Персонаж", "Настройки мода", "Управление отрядом",
              "Журнал событий", "Достижения", "Помощь", "Выход из мода"]
    for index, label in enumerate(labels):
        y = 115 + index * 42
        if index == active:
            rect(16, y, 226, 39, (73, 20, 42, 190), 4)
            rect(16, y, 3, 39, PINK, 2)
        text("●", 39, y + 19, 9, PINK if index == active else MUTED, anchor="mm")
        text(label, 63, y + 11, 11, WHITE if index == active else MUTED)
    text("PROJECT LIBERTY v1.0.0", 28, 650, 8, MUTED)
    text("Nothing Is Scripted", 28, 670, 10, PINK)
    rect(28, 691, 168, 1, PINK)


def empty_dashboard(width, height, logo):
    scale = height / 720
    bg = Image.open(ASSETS / "dashboard-background.png").convert("RGB")
    bg = bg.resize((width, height), Image.Resampling.LANCZOS)
    bg = ImageEnhance.Brightness(bg).enhance(0.58).filter(ImageFilter.GaussianBlur(0.25)).convert("RGBA")
    image = Image.alpha_composite(bg, Image.new("RGBA", (width, height), (0, 5, 10, 112)))
    rect, text = helpers(image, scale)
    sidebar(image, logo, scale, 0)
    rect(258, 0, 1022, 720, (3, 8, 13, 88))
    text("ГЛАВНАЯ", 279, 43, 17, WHITE, True)
    text("Добро пожаловать в Project Liberty", 279, 67, 10, MUTED)
    rect(278, 105, 972, 575, (4, 8, 13, 196), 8, BORDER)
    draw_logo(image, logo, scale, 665, 140, 110, 115)
    text("С О З Д А Й Т Е   С В О Е Г О", 764, 288, 17, WHITE, False, "ma")
    text("П Е Р С О Н А Ж А", 764, 330, 34, PINK, True, "ma")
    rect(580, 372, 368, 1, PINK)
    text("Начните карьеру в Лос-Сантосе", 764, 402, 13, MUTED, False, "ma")
    text("Создайте уникального персонажа Project Liberty", 764, 426, 10, MUTED, False, "ma")
    rect(418, 486, 692, 106, (32, 8, 20, 235), 10, PINK, 2)
    rect(448, 507, 64, 64, (10, 15, 20, 255), 8, PINK)
    text("+", 480, 539, 31, PINK, False, "mm")
    text("С О З Д А Т Ь   П Е Р С О Н А Ж А", 739, 538, 18, WHITE, True, "mm")
    text("›", 1068, 538, 38, PINK, False, "mm")
    return image.convert("RGB")


def editor_shell(width, height, logo):
    scale = height / 720
    image = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    rect, text = helpers(image, scale)
    rect(0, 0, 270, 720, PANEL)
    draw_logo(image, logo, scale, 24, 20, 55, 60)
    text("P R O J E C T", 84, 29, 17, WHITE, True)
    text("L I B E R T Y", 84, 52, 17, PINK, True)
    text("Hard RP Mod", 84, 78, 9, MUTED)
    sections = ["Внешность", "Одежда", "Имя и данные", "Предпросмотр"]
    for i, label in enumerate(sections):
        y = 128 + i * 48
        text("●", 43, y + 20, 9, MUTED, anchor="mm")
        text(label, 67, y + 12, 11, MUTED)
    rect(24, 476, 228, 143, (8, 13, 17, 230), 6, BORDER)
    text("ИНФОРМАЦИЯ", 68, 498, 11, WHITE, True)
    text("Только возможности", 43, 530, 9, MUTED)
    text("редактора GTA V.", 43, 550, 9, MUTED)
    text("Доступно 3 персонажа.", 43, 580, 9, MUTED)
    rect(24, 650, 228, 45, CARD, 5, BORDER)
    text("ESC   ВЕРНУТЬСЯ В МЕНЮ", 138, 673, 10, WHITE, False, "mm")

    # Step indicator and top-right slot card.
    steps = [(460, "ВНЕШНОСТЬ"), (575, "ОДЕЖДА"), (690, "ИМЯ И ДАННЫЕ"), (805, "ПРОСМОТР")]
    for i, (x, label) in enumerate(steps):
        rect(x-17, 24, 34, 34, (7, 12, 16, 225), 17, BORDER)
        text(str(i+1), x, 41, 10, MUTED, True, "mm")
        if i < 3: rect(x+25, 40, 65, 1, BORDER)
    rect(1022, 18, 235, 60, (6, 11, 15, 235), 6, BORDER)
    draw_logo(image, logo, scale, 1035, 27, 38, 42)
    text("ПЕРСОНАЖ", 1085, 30, 10, WHITE, True)

    # Right-side live values are drawn by C# above this static frame.
    rect(895, 102, 362, 542, (5, 10, 14, 242), 7, BORDER)
    rect(919, 146, 314, 1, BORDER)
    for y in (174, 213, 252, 291, 330, 369, 408, 447):
        rect(1090, y-10, 143, 28, CARD, 4, BORDER)
        text("‹", 1105, y+4, 16, PINK, False, "mm")
        text("›", 1218, y+4, 16, PINK, False, "mm")
    rect(919, 576, 314, 47, PINK, 5)
    rect(385, 650, 430, 45, (5, 10, 14, 220), 6, BORDER)
    text("ЛКМ: выбор     Колесо: масштаб     Перетаскивание: поворот", 600, 673, 9, MUTED, False, "mm")
    return image


logo = Image.open(ASSETS / "project-liberty-logo.png").convert("RGBA")
for width, height in RESOLUTIONS:
    empty_dashboard(width, height, logo).save(ASSETS / f"empty-dashboard-{width}x{height}.png", optimize=True)
    editor_shell(width, height, logo).save(ASSETS / f"character-editor-{width}x{height}.png", optimize=True)

print("Generated empty dashboard and character editor assets for 720p, 1080p, 1440p and 4K")
