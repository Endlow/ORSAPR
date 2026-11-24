# -*- coding: utf-8 -*-
"""Генератор макетов пользовательского интерфейса для проекта системы.

Макеты рисуются в стиле WinForms и подписывают области, требуемые
методичкой: описание параметров и диапазонов, ввод параметров,
управление и состояние. Запуск: python render_mockups.py
"""

import os
from PIL import Image, ImageDraw, ImageFont

S = 2  # суперсэмплинг для сглаживания
OUT = os.path.dirname(os.path.abspath(__file__))

FONT_DIR = "/usr/share/fonts/truetype/dejavu"
FONT_REG = os.path.join(FONT_DIR, "DejaVuSans.ttf")
FONT_BOLD = os.path.join(FONT_DIR, "DejaVuSans-Bold.ttf")
FONT_SERIF = os.path.join(FONT_DIR, "DejaVuSerif.ttf")

CLIENT = (240, 240, 240)
BORDER = (173, 173, 173)
DARK = (0, 0, 0)
GRAY = (90, 90, 90)
BLUE = (0, 120, 215)
BLUE_DARK = (0, 90, 158)
AREA = (31, 78, 121)
RED = (200, 30, 30)
FIELD_ERROR = (253, 232, 232)


def font(path, size):
    return ImageFont.truetype(path, size * S)


def pt(v):
    return int(round(v * S))


class Drawer:
    """Обёртка над ImageDraw, позволяющая передавать линию четырьмя числами."""

    def __init__(self, draw):
        self._draw = draw

    def __getattr__(self, name):
        return getattr(self._draw, name)

    def line(self, xy, fill=None, width=1):
        if len(xy) == 4 and all(isinstance(v, (int, float)) for v in xy):
            xy = [(xy[0], xy[1]), (xy[2], xy[3])]
        return self._draw.line(xy, fill=fill, width=width)


def canvas(width, height):
    image = Image.new("RGB", (pt(width), pt(height)), (255, 255, 255))
    return image, Drawer(ImageDraw.Draw(image))


def text(draw, x, y, value, fnt, fill=DARK, anchor="la"):
    draw.text((pt(x), pt(y)), value, font=fnt, fill=fill, anchor=anchor)


def dashed_line(draw, x1, y1, x2, y2, fill, width=1, dash=5, gap=4):
    x1, y1, x2, y2 = pt(x1), pt(y1), pt(x2), pt(y2)
    length = ((x2 - x1) ** 2 + (y2 - y1) ** 2) ** 0.5
    if length == 0:
        return
    dx = (x2 - x1) / length
    dy = (y2 - y1) / length
    pos = 0
    while pos < length:
        end = min(pos + dash * S, length)
        draw.line((x1 + dx * pos, y1 + dy * pos, x1 + dx * end, y1 + dy * end),
                  fill=fill, width=pt(width))
        pos += (dash + gap) * S


def dashed_rect(draw, x, y, w, h, fill, width=1):
    dashed_line(draw, x, y, x + w, y, fill, width)
    dashed_line(draw, x + w, y, x + w, y + h, fill, width)
    dashed_line(draw, x + w, y + h, x, y + h, fill, width)
    dashed_line(draw, x, y + h, x, y, fill, width)


def window(draw, x, y, w, h, title, title_fnt):
    # Рамка и заголовок окна
    draw.rectangle((pt(x), pt(y), pt(x + w), pt(y + h)), fill=CLIENT, outline=(120, 120, 120), width=pt(1))
    draw.rectangle((pt(x + 1), pt(y + 1), pt(x + w - 1), pt(y + 36)), fill=(255, 255, 255))
    draw.line((pt(x + 1), pt(y + 36), pt(x + w - 1), pt(y + 36)), fill=BORDER, width=pt(1))
    text(draw, x + 12, y + 9, title, title_fnt, DARK)

    # Кнопки управления окном
    cx = x + w - 26
    draw.line((pt(cx), pt(y + 12), pt(cx + 10), pt(y + 22)), fill=DARK, width=pt(1))
    draw.line((pt(cx + 10), pt(y + 12), pt(cx), pt(y + 22)), fill=DARK, width=pt(1))
    draw.line((pt(cx - 24), pt(y + 21), pt(cx - 14), pt(y + 21)), fill=DARK, width=pt(1))
    draw.rectangle((pt(cx - 46), pt(y + 14), pt(cx - 38), pt(y + 22)), outline=DARK, width=pt(1))


def area_label(draw, x, y, value, small_fnt):
    text(draw, x, y, value, small_fnt, AREA)
    draw.line((pt(x), pt(y + 17), pt(x + 210), pt(y + 17)), fill=(31, 78, 121), width=pt(1))


def group_box(draw, x, y, w, h, title, label_fnt):
    draw.rectangle((pt(x), pt(y), pt(x + w), pt(y + h)), outline=BORDER, width=pt(1))
    tw = draw.textlength(title, font=label_fnt)
    draw.rectangle((pt(x + 9), pt(y - 8), pt(x + 9) + tw + pt(8), pt(y + 8)), fill=CLIENT)
    text(draw, x + 12, y - 8, title, label_fnt, (31, 78, 121))


def numeric(draw, x, y, w, h, value, value_fnt, error=False):
    fill = FIELD_ERROR if error else (255, 255, 255)
    draw.rectangle((pt(x), pt(y), pt(x + w), pt(y + h)), fill=fill, outline=RED if error else (122, 122, 122), width=pt(1))
    text(draw, x + 6, y + h / 2, value, value_fnt, DARK, anchor="lm")

    bw = 16
    draw.rectangle((pt(x + w - bw), pt(y + 1), pt(x + w - 1), pt(y + h / 2)), fill=(235, 235, 235), outline=(122, 122, 122), width=pt(1))
    draw.rectangle((pt(x + w - bw), pt(y + h / 2), pt(x + w - 1), pt(y + h - 1)), fill=(235, 235, 235), outline=(122, 122, 122), width=pt(1))
    # стрелки
    ax = x + w - bw / 2
    draw.polygon([(pt(ax - 4), pt(y + h / 2 - 2)), (pt(ax + 4), pt(y + h / 2 - 2)), (pt(ax), pt(y + 5))], fill=(60, 60, 60))
    draw.polygon([(pt(ax - 4), pt(y + h / 2 + 2)), (pt(ax + 4), pt(y + h / 2 + 2)), (pt(ax), pt(y + h - 5))], fill=(60, 60, 60))


def checkbox(draw, x, y, label, label_fnt, checked=False, enabled=True):
    color = DARK if enabled else (160, 160, 160)
    draw.rectangle((pt(x), pt(y), pt(x + 14), pt(y + 14)), fill=(255, 255, 255), outline=color, width=pt(1))
    if checked:
        draw.line((pt(x + 3), pt(y + 7), pt(x + 6), pt(y + 11)), fill=color, width=pt(2))
        draw.line((pt(x + 6), pt(y + 11), pt(x + 12), pt(y + 3)), fill=color, width=pt(2))
    text(draw, x + 21, y - 1, label, label_fnt, color)


def button(draw, x, y, w, h, label, btn_fnt, primary=False):
    fill = BLUE if primary else (225, 225, 225)
    outline = BLUE_DARK if primary else (160, 160, 160)
    fg = (255, 255, 255) if primary else DARK
    draw.rectangle((pt(x), pt(y), pt(x + w), pt(y + h)), fill=fill, outline=outline, width=pt(1))
    text(draw, x + w / 2, y + h / 2, label, btn_fnt, fg, anchor="mm")


def tooltip(draw, x, y, w, value, small_fnt):
    h = 54
    draw.rounded_rectangle((pt(x), pt(y), pt(x + w), pt(y + h)), radius=pt(6), fill=(255, 249, 196), outline=(184, 166, 0), width=pt(1))
    # указатель
    draw.polygon([(pt(x + 20), pt(y)), (pt(x + 32), pt(y)), (pt(x + 26), pt(y - 9))], fill=(255, 249, 196), outline=(184, 166, 0))
    draw.line([(pt(x + 22), pt(y)), (pt(x + 30), pt(y))], fill=(255, 249, 196), width=pt(2))
    text(draw, x + 10, y + 8, value, small_fnt, DARK)


def error_dialog(draw, x, y, w, h, title, lines, title_fnt, body_fnt, btn_fnt):
    draw.rectangle((pt(x), pt(y), pt(x + w), pt(y + h)), fill=(255, 255, 255), outline=(120, 120, 120), width=pt(1))
    draw.rectangle((pt(x + 1), pt(y + 1), pt(x + w - 1), pt(y + 34)), fill=(255, 255, 255))
    draw.line((pt(x + 1), pt(y + 34), pt(x + w - 1), pt(y + 34)), fill=BORDER, width=pt(1))
    text(draw, x + 10, y + 8, title, title_fnt, DARK)
    # крестик
    cx, cy, r = x + w - 18, y + 16, 7
    draw.line((pt(cx - r), pt(cy - r), pt(cx + r), pt(cy + r)), fill=DARK, width=pt(1))
    draw.line((pt(cx + r), pt(cy - r), pt(cx - r), pt(cy + r)), fill=DARK, width=pt(1))

    # красный значок ошибки
    ix, iy, ir = x + 42, y + 74, 20
    draw.ellipse((pt(ix - ir), pt(iy - ir), pt(ix + ir), pt(iy + ir)), fill=(214, 39, 40))
    draw.line((pt(ix - 10), pt(iy - 10), pt(ix + 10), pt(iy + 10)), fill=(255, 255, 255), width=pt(4))
    draw.line((pt(ix + 10), pt(iy - 10), pt(ix - 10), pt(iy + 10)), fill=(255, 255, 255), width=pt(4))

    ty = y + 58
    for line in lines:
        text(draw, x + 82, ty, line, body_fnt, DARK)
        ty += 22

    button(draw, x + w - 106, y + h - 46, 90, 30, "OK", btn_fnt)


def parameter_rows(draw, x, y, values, label_fnt, value_fnt, error_index=None, tooltip_text=None):
    rows = [
        ("Длина опоры L, мм (60–150)", values[0]),
        ("Ширина опоры W, мм (50–100)", values[1]),
        ("Высота опоры H, мм (100–300)", values[2]),
        ("Длина паза d, мм (20–100)", values[3]),
        ("Высота паза h, мм (20–80)", values[4]),
        ("Глубина паза p, мм (20–80)", values[5]),
    ]
    step = 40
    for i, (label, value) in enumerate(rows):
        ry = y + i * step
        text(draw, x, ry, label, label_fnt, DARK)
        numeric(draw, x + 386, ry - 3, 140, 28, value, value_fnt, error=(i == error_index))
        if i == error_index and tooltip_text:
            tooltip(draw, x + 386, ry - 63, 360, tooltip_text, label_fnt)


def main_window(draw, x, y, w, h, title, title_fnt, label_fnt, small_fnt, value_fnt, btn_fnt,
                values=("80,0", "60,0", "120,0", "40,0", "50,0", "30,0"),
                error_index=None, tooltip_text=None, status_text=None):
    window(draw, x, y, w, h, title, title_fnt)

    group_box(draw, x + 18, y + 46, w - 36, 324, "Параметры опоры", label_fnt)

    # Подписи областей
    text(draw, x + 32, y + 72, "ОБЛАСТЬ ОПИСАНИЯ ПАРАМЕТРОВ И ДИАПАЗОНОВ", small_fnt, AREA)
    draw.line((pt(x + 32), pt(y + 89), pt(x + 372), pt(y + 89)), fill=AREA, width=pt(1))
    text(draw, x + 414, y + 72, "ОБЛАСТЬ ВВОДА ПАРАМЕТРОВ", small_fnt, AREA)
    draw.line((pt(x + 414), pt(y + 89), pt(x + 690), pt(y + 89)), fill=AREA, width=pt(1))

    parameter_rows(draw, x + 32, y + 100, values, label_fnt, value_fnt, error_index, tooltip_text)
    checkbox(draw, x + 32, y + 344, "Сквозной паз (вырез на всю ширину W)", label_fnt, checked=False)

    # Область управления
    text(draw, x + 18, y + 384, "ОБЛАСТЬ УПРАВЛЕНИЯ", small_fnt, AREA)
    draw.line((pt(x + 18), pt(y + 401), pt(x + 700), pt(y + 401)), fill=AREA, width=pt(1))

    button(draw, x + 18, y + 412, 130, 34, "Сбросить", btn_fnt)
    button(draw, x + 158, y + 412, 170, 34, "Показать эскиз", btn_fnt)
    button(draw, x + 338, y + 412, 362, 34, "Построить модель", btn_fnt, primary=True)

    # Область состояния
    text(draw, x + 18, y + 462, "ОБЛАСТЬ СОСТОЯНИЯ", small_fnt, AREA)
    draw.line((pt(x + 18), pt(y + 479), pt(x + 700), pt(y + 479)), fill=AREA, width=pt(1))
    text(draw, x + 18, y + 486, status_text or
         "Значения по умолчанию. Измените параметры и нажмите «Построить модель».",
         small_fnt, GRAY)


def save(image, name):
    image = image.resize((image.width // S, image.height // S), Image.LANCZOS)
    path = os.path.join(OUT, name)
    image.save(path)
    print("saved", path, image.size)


def main():
    label_fnt = font(FONT_REG, 12)
    small_fnt = font(FONT_REG, 9)
    value_fnt = font(FONT_REG, 12)
    btn_fnt = font(FONT_REG, 12)
    title_fnt = font(FONT_REG, 13)

    # 1. Главное окно
    img, d = canvas(760, 540)
    main_window(d, 10, 10, 740, 520, "Опора с пазом — КОМПАС-3D", title_fnt,
                label_fnt, small_fnt, value_fnt, btn_fnt)
    save(img, "SupportMainWindow.png")

    # 2. Ошибка валидации
    img, d = canvas(760, 540)
    main_window(d, 10, 10, 740, 520, "Опора с пазом — КОМПАС-3D", title_fnt,
                label_fnt, small_fnt, value_fnt, btn_fnt,
                values=("40,0", "60,0", "120,0", "40,0", "50,0", "30,0"),
                error_index=0,
                tooltip_text="Значение длины опоры L = 40 мм выходит\nза допустимый диапазон [60; 150] мм.",
                status_text="Проверьте значения параметров.")
    error_dialog(d, 210, 255, 530, 210, "Ошибка построения",
                 ["Проверьте параметры:",
                  "",
                  "Значение длины опоры L = 40 мм выходит за",
                  "допустимый диапазон [60; 150] мм."],
                 title_fnt, label_fnt, btn_fnt)
    save(img, "SupportValidationError.png")

    # 3. Ошибка построения
    img, d = canvas(760, 540)
    main_window(d, 10, 10, 740, 520, "Опора с пазом — КОМПАС-3D", title_fnt,
                label_fnt, small_fnt, value_fnt, btn_fnt,
                status_text="Запуск КОМПАС-3D и построение модели…")
    error_dialog(d, 190, 250, 550, 220, "Ошибка построения",
                 ["Не удалось построить модель:",
                  "",
                  "Не удалось сохранить файл модели по указанному пути.",
                  "Проверьте, что папка назначения существует",
                  "и доступна для записи."],
                 title_fnt, label_fnt, btn_fnt)
    save(img, "SupportBuildError.png")

    # 4. Эскиз
    img, d = canvas(720, 520)
    window(d, 10, 10, 700, 500, "Эскиз с размерами опоры", title_fnt)
    text(d, 28, 56, "ОБЛАСТЬ ЭСКИЗА С РАЗМЕРАМИ", small_fnt, AREA)
    d.line((pt(28), pt(73), pt(692), pt(73)), fill=AREA, width=pt(1))

    # геометрия: вид спереди, паз на правой грани
    bx, by, bw, bh = 180, 92, 380, 360
    d.rectangle((pt(bx), pt(by), pt(bx + bw), pt(by + bh)), fill=(205, 218, 237), outline=DARK, width=pt(2))
    gx, gy, gw, gh = bx + bw - 90, by + 100, 90, 120
    d.rectangle((pt(gx), pt(gy), pt(gx + gw), pt(gy + gh)), fill=(255, 255, 255), outline=RED, width=pt(2))

    dim_fnt = font(FONT_SERIF, 12)
    # L
    d.line((pt(bx), pt(by + bh + 26), pt(bx + bw), pt(by + bh + 26)), fill=GRAY, width=pt(1))
    d.line((pt(bx), pt(by + bh + 20), pt(bx), pt(by + bh + 32)), fill=GRAY, width=pt(1))
    d.line((pt(bx + bw), pt(by + bh + 20), pt(bx + bw), pt(by + bh + 32)), fill=GRAY, width=pt(1))
    text(d, bx + bw / 2, by + bh + 32, "L = 80", dim_fnt, DARK, anchor="ma")
    # H
    d.line((pt(bx - 26), pt(by), pt(bx - 26), pt(by + bh)), fill=GRAY, width=pt(1))
    d.line((pt(bx - 32), pt(by), pt(bx - 20), pt(by)), fill=GRAY, width=pt(1))
    d.line((pt(bx - 32), pt(by + bh), pt(bx - 20), pt(by + bh)), fill=GRAY, width=pt(1))
    text(d, bx - 36, by + bh / 2, "H = 120", dim_fnt, DARK, anchor="rm")
    # W
    text(d, bx + bw + 10, by + 6, "W = 60", dim_fnt, GRAY)
    # p (глубина)
    d.line((pt(gx), pt(gy - 16), pt(gx + gw), pt(gy - 16)), fill=GRAY, width=pt(1))
    text(d, gx + gw / 2, gy - 32, "p = 30", dim_fnt, DARK, anchor="ma")
    # d (длина паза)
    d.line((pt(gx), pt(gy + gh + 18), pt(gx + gw), pt(gy + gh + 18)), fill=GRAY, width=pt(1))
    text(d, gx + gw / 2, gy + gh + 22, "d = 40", dim_fnt, DARK, anchor="ma")
    # h (высота паза)
    d.line((pt(gx + gw + 14), pt(gy), pt(gx + gw + 14), pt(gy + gh)), fill=GRAY, width=pt(1))
    text(d, gx + gw + 20, gy + gh / 2, "h = 50", dim_fnt, RED, anchor="lm")
    text(d, 28, 82, "M1 = 10, M2 = 20, M3 = 10", dim_fnt, GRAY)
    save(img, "SupportSketchWindow.png")


if __name__ == "__main__":
    main()
