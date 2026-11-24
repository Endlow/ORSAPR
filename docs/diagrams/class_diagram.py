# -*- coding: utf-8 -*-
"""UML-диаграмма классов плагина «Опора с пазом»: раскладка и рендер.

Диаграмма переработана: компактная двухколоночная раскладка, крупные подписи
(читаемы в Word при 100 %), подписанные связи (композиция / использование)
и кратности. Исходник для редактирования — SupportClassDiagram.drawio,
он строится из той же раскладки (make_drawio.py).
"""

import os

from PIL import Image, ImageDraw, ImageFont

S = 2  # суперсэмплинг
OUT_DIR = os.path.dirname(os.path.abspath(__file__))
FONT_DIR = "/usr/share/fonts/truetype/dejavu"
FONT_REG = os.path.join(FONT_DIR, "DejaVuSans.ttf")
FONT_BOLD = os.path.join(FONT_DIR, "DejaVuSans-Bold.ttf")

WHITE = (255, 255, 255)
BORDER = (47, 79, 111)
HEADER = (220, 230, 242)
TEXT = (20, 20, 20)
MUTED = (70, 70, 70)
COMP = (31, 78, 121)
USE = (120, 80, 30)

SIZE_NAME = 24
SIZE_MEMBER = 19
SIZE_REL = 18
SIZE_MULT = 16

PAD_X = 16
LINE_H = 27
HEADER_H = 46
SEP_H = 16
PAD_BOTTOM = 22
GAP_X = 150
GAP_Y = 64
MARGIN = 36
MAX_TEXT_W = 460  # ширина строки в пикселях, до которой переносятся члены

CLASSES = [
    dict(id="mainForm", name="MainForm", col=0,
         fields=["- _parametersControl : SupportParametersControl",
                 "- _builder : SupportBuilder",
                 "- _statusLabel : Label"],
         methods=["+ MainForm()",
                  "- OnBuildModelClick(sender, e) : void",
                  "- ValidateParameters(parameters) : IReadOnlyList<ValidationError>",
                  "- BuildModel(parameters) : void",
                  "- ShowError(message) : void",
                  "# OnFormClosed(e) : void"]),
    dict(id="control", name="SupportParametersControl", col=0,
         fields=["- _fields : Dictionary<SupportParameter, NumericUpDown>",
                 "- _toolTip : ToolTip"],
         methods=["+ SupportParametersControl()",
                  "+ GetParameters() : SupportParameters",
                  "+ SetParameters(parameters) : void",
                  "+ SetDefault() : void",
                  "+ ClearErrors() : void",
                  "+ SetError(parameter, message) : void",
                  "+ ApplyErrors(errors) : void"]),
    dict(id="parameters", name="SupportParameters", col=0,
         fields=["+ SupportLength : double",
                 "+ SupportWidth : double",
                 "+ SupportHeight : double",
                 "+ GrooveLength : double",
                 "+ GrooveHeight : double",
                 "+ GrooveDepth : double",
                 "+ ThroughSlot : bool",
                 "+ GrooveEndMargin : double = 10",
                 "+ GrooveVerticalMargin : double = 20",
                 "+ GrooveBackMargin : double = 10"],
         methods=["+ Default() : SupportParameters",
                  "+ Validate() : IReadOnlyList<ValidationError>"]),
    dict(id="validation", name="ValidationError", col=0,
         fields=["+ Parameter : SupportParameter",
                 "+ Message : string"],
         methods=["+ ValidationError(parameter, message)"]),
    dict(id="builder", name="SupportBuilder", col=1,
         fields=["- _wrapper : Kompas3DWrapper"],
         methods=["+ SupportBuilder()",
                  "+ SupportBuilder(wrapper)",
                  "+ Build(parameters, path) : string",
                  "- BuildBase(parameters) : void",
                  "- BuildSlot(parameters) : void",
                  "+ SaveModel(path) : void",
                  "+ CloseModel() : void",
                  "+ Dispose() : void"]),
    dict(id="wrapper", name="Kompas3DWrapper", col=1,
         fields=["- _launcher : KompasLauncher",
                 "- _document3D : ksDocument3D",
                 "- _part : ksPart"],
         methods=["+ Kompas3DWrapper()",
                  "+ Kompas3DWrapper(launcher)",
                  "+ ConnectOrStart() : void",
                  "+ CreateNewPartDocument() : void",
                  "+ CreateSketch(planeType) : SketchSession",
                  "+ BaseExtrude(sketch, depth) : void",
                  "+ CutExtrude(sketch, depth) : void",
                  "+ SetIsoView() : void",
                  "+ SaveAs(path) : void",
                  "+ CloseDocument() : void",
                  "+ Dispose() : void"]),
    dict(id="launcher", name="KompasLauncher", col=1,
         fields=["- _kompas : KompasObject",
                 "+ Kompas : KompasObject",
                 "+ IsConnected : bool"],
         methods=["+ ConnectOrStart() : void",
                  "+ Disconnect() : void",
                  "+ Dispose() : void"]),
]

# (источник, приёмник, подпись, вид связи)
EDGES = [
    ("mainForm", "control", "композиция", "composition"),
    ("mainForm", "parameters", "использует", "use"),
    ("mainForm", "builder", "композиция", "composition"),
    ("control", "parameters", "использует", "use"),
    ("parameters", "validation", "использует", "use"),
    ("builder", "parameters", "использует", "use"),
    ("builder", "wrapper", "композиция", "composition"),
    ("wrapper", "launcher", "композиция", "composition"),
]

MULTIPLICITIES = {
    ("mainForm", "control"): ("1", "1"),
    ("mainForm", "parameters"): ("1", "1"),
    ("mainForm", "builder"): ("1", "1"),
    ("control", "parameters"): ("1", "1"),
    ("parameters", "validation"): ("1", "0..*"),
    ("builder", "parameters"): ("1", "1"),
    ("builder", "wrapper"): ("1", "1"),
    ("wrapper", "launcher"): ("1", "1"),
}

COLUMN_ORDER = {0: ["mainForm", "control", "parameters", "validation"],
                1: ["builder", "wrapper", "launcher"]}


def pt(v):
    return int(round(v * S))


def font(path, size):
    return ImageFont.truetype(path, size * S)


def text_width(draw, value, fnt):
    return draw.textlength(value, font=fnt) / S


def wrap(draw, value, fnt, limit):
    """Переносит строку по словам, если она шире limit."""
    if text_width(draw, value, fnt) <= limit:
        return [value]
    words = value.split(" ")
    lines = []
    current = ""
    for word in words:
        candidate = word if not current else current + " " + word
        if text_width(draw, candidate, fnt) <= limit or not current:
            current = candidate
        else:
            lines.append(current)
            current = word
    if current:
        lines.append(current)
    return lines


def measure(draw, spec, name_fnt, member_fnt):
    fields = []
    for line in spec["fields"]:
        fields.extend(wrap(draw, line, member_fnt, MAX_TEXT_W))
    methods = []
    for line in spec["methods"]:
        methods.extend(wrap(draw, line, member_fnt, MAX_TEXT_W))
    width = max([text_width(draw, spec["name"], name_fnt)] +
                [text_width(draw, line, member_fnt) for line in fields + methods])
    height = (HEADER_H + len(fields) * LINE_H + SEP_H +
              len(methods) * LINE_H + PAD_BOTTOM)
    return dict(fields=fields, methods=methods,
                w=width + 2 * PAD_X, h=height)


def layout():
    """Считает координаты классов и размеры холста (в логических пикселях)."""
    probe = ImageDraw.Draw(Image.new("RGB", (10, 10)))
    name_fnt = font(FONT_BOLD, SIZE_NAME)
    member_fnt = font(FONT_REG, SIZE_MEMBER)

    boxes = {}
    for spec in CLASSES:
        boxes[spec["id"]] = measure(probe, spec, name_fnt, member_fnt)

    col_width = {0: 0, 1: 0}
    for spec in CLASSES:
        col_width[spec["col"]] = max(col_width[spec["col"]],
                                     boxes[spec["id"]]["w"])

    x0 = MARGIN
    x1 = MARGIN + col_width[0] + GAP_X
    for spec in CLASSES:
        box = boxes[spec["id"]]
        box["name"] = spec["name"]
        box["x"] = x0 if spec["col"] == 0 else x1
        box["w"] = col_width[spec["col"]]  # общая ширина колонки
        box["y"] = 0

    for col, ids in COLUMN_ORDER.items():
        cursor = MARGIN
        for cid in ids:
            boxes[cid]["y"] = cursor
            cursor += boxes[cid]["h"] + GAP_Y

    width = MARGIN + col_width[0] + GAP_X + col_width[1] + MARGIN
    height = max(boxes["validation"]["y"] + boxes["validation"]["h"],
                 boxes["launcher"]["y"] + boxes["launcher"]["h"]) + MARGIN
    return boxes, width, height


class Canvas:
    """ImageDraw с логическими координатами (умножаются на S)."""

    def __init__(self, draw, image):
        self._draw = draw
        self._image = image

    def __getattr__(self, name):
        return getattr(self._draw, name)


def class_box(draw, box, name_fnt, member_fnt):
    x, y, w, h = box["x"], box["y"], box["w"], box["h"]
    draw.rectangle((pt(x), pt(y), pt(x + w), pt(y + h)), fill=WHITE,
                   outline=BORDER, width=pt(2))
    draw.rectangle((pt(x + 1), pt(y + 1), pt(x + w - 1), pt(y + HEADER_H)),
                   fill=HEADER)
    draw.line([(pt(x), pt(y + HEADER_H)), (pt(x + w), pt(y + HEADER_H))],
              fill=BORDER, width=pt(2))
    draw.text((pt(x + w / 2), pt(y + 12)), box["name"], font=name_fnt,
              fill=TEXT, anchor="ma")

    ty = y + HEADER_H + 8
    for line in box["fields"]:
        draw.text((pt(x + PAD_X), pt(ty)), line, font=member_fnt, fill=TEXT)
        ty += LINE_H

    ty += SEP_H
    draw.line([(pt(x), pt(ty - SEP_H / 2)), (pt(x + w), pt(ty - SEP_H / 2))],
              fill=(180, 190, 200), width=pt(1))
    for line in box["methods"]:
        draw.text((pt(x + PAD_X), pt(ty)), line, font=member_fnt, fill=TEXT)
        ty += LINE_H


def dashed(draw, points, fill, width=2, dash=7, gap=5):
    for (x1, y1), (x2, y2) in zip(points, points[1:]):
        x1, y1, x2, y2 = pt(x1), pt(y1), pt(x2), pt(y2)
        length = ((x2 - x1) ** 2 + (y2 - y1) ** 2) ** 0.5
        if length == 0:
            continue
        dx, dy = (x2 - x1) / length, (y2 - y1) / length
        pos = 0
        while pos < length:
            end = min(pos + dash * S, length)
            draw.line([(x1 + dx * pos, y1 + dy * pos),
                       (x1 + dx * end, y1 + dy * end)], fill=fill, width=pt(width))
            pos += (dash + gap) * S


def diamond(draw, x, y, direction, fill=COMP, size=11):
    x, y, size = pt(x), pt(y), pt(size)
    pts = {
        "left": [(x - size, y), (x, y - size), (x + size, y), (x, y + size)],
        "right": [(x + size, y), (x, y - size), (x - size, y), (x, y + size)],
        "up": [(x, y - size), (x - size, y), (x, y + size), (x + size, y)],
        "down": [(x, y + size), (x - size, y), (x, y - size), (x + size, y)],
    }[direction]
    draw.polygon(pts, fill=fill)


def open_arrow(draw, x, y, direction, fill=USE, size=9):
    x, y, size = pt(x), pt(y), pt(size)
    if direction == "down":
        pts = [(x - size, y - size * 1.4), (x, y), (x + size, y - size * 1.4)]
    elif direction == "up":
        pts = [(x - size, y + size * 1.4), (x, y), (x + size, y + size * 1.4)]
    elif direction == "left":
        pts = [(x + size * 1.4, y - size), (x, y), (x + size * 1.4, y + size)]
    else:
        pts = [(x - size * 1.4, y - size), (x, y), (x - size * 1.4, y + size)]
    draw.line(pts, fill=fill, width=pt(2))


def label(draw, x, y, value, fnt, fill=MUTED, anchor="lm"):
    """Подпись на белой подложке, чтобы линия не проходила по тексту."""
    w = draw.textlength(value, font=fnt)
    h = fnt.size
    cx, cy = pt(x), pt(y)
    if anchor == "lm":
        box = (cx - 4 * S, cy - h / 2 - 2 * S, cx + w + 4 * S, cy + h / 2 + 2 * S)
    elif anchor == "rm":
        box = (cx - w - 4 * S, cy - h / 2 - 2 * S, cx + 4 * S, cy + h / 2 + 2 * S)
    else:  # mm
        box = (cx - w / 2 - 4 * S, cy - h / 2 - 2 * S,
               cx + w / 2 + 4 * S, cy + h / 2 + 2 * S)
    draw.rectangle(box, fill=WHITE)
    draw.text((cx, cy), value, font=fnt, fill=fill, anchor=anchor)


def label_vertical(draw, x, y, value, fnt, fill=MUTED):
    """Подпись, повёрнутая на 90° (для вертикальных участков связи)."""
    temp = Image.new("RGBA", (400, 60), (255, 255, 255, 0))
    ImageDraw.Draw(temp).text((10, 30), value, font=fnt, fill=fill, anchor="lm")
    temp = temp.rotate(90, expand=True)
    draw._image.paste(temp, (pt(x) - temp.width // 2, pt(y) - temp.height // 2),
                      temp)


def vertical_edge(draw, src, dst, kind, text, m1, m2, x, rel_fnt, mult_fnt):
    """Вертикальная связь между классами одной колонки."""
    y1 = src["y"] + src["h"]
    y2 = dst["y"]
    if kind == "composition":
        draw.line([(pt(x), pt(y1)), (pt(x), pt(y2))], fill=COMP, width=pt(2))
        diamond(draw, x, y1, "down")
    else:
        dashed(draw, [(x, y1), (x, y2)], USE, 2)
        open_arrow(draw, x, y2, "down")
    label(draw, x + 16, (y1 + y2) / 2, text, rel_fnt)
    label(draw, x - 16, y1 + 18, m1, mult_fnt, anchor="rm")
    label(draw, x - 16, y2 - 18, m2, mult_fnt, anchor="rm")


def draw_edges(draw, boxes, gap_center):
    rel_fnt = font(FONT_REG, SIZE_REL)
    mult_fnt = font(FONT_REG, SIZE_MULT)
    mf, ctl = boxes["mainForm"], boxes["control"]
    prm, ve = boxes["parameters"], boxes["validation"]
    bld, wrp, lnch = boxes["builder"], boxes["wrapper"], boxes["launcher"]

    # 1. MainForm —композиция→ SupportParametersControl
    vertical_edge(draw, mf, ctl, "composition", "композиция", "1", "1",
                  mf["x"] + 80, rel_fnt, mult_fnt)

    # 2. MainForm ..> SupportParameters (вертикаль в межколонном зазоре)
    xa = gap_center - 20
    y_start = mf["y"] + mf["h"] - 46
    y_end = prm["y"] + 62
    dashed(draw, [(mf["x"] + mf["w"], y_start), (xa, y_start),
                  (xa, y_end), (prm["x"] + prm["w"], y_end)], USE, 2)
    open_arrow(draw, prm["x"] + prm["w"], y_end, "left")
    label_vertical(draw, xa - 22, (y_start + y_end) / 2, "использует", rel_fnt)
    label(draw, mf["x"] + mf["w"] + 14, y_start - 18, "1", mult_fnt)
    label(draw, prm["x"] + prm["w"] + 14, y_end - 18, "1", mult_fnt)

    # 3. MainForm —композиция→ SupportBuilder
    y = mf["y"] + 60
    draw.line([(pt(mf["x"] + mf["w"]), pt(y)), (pt(bld["x"]), pt(y))],
              fill=COMP, width=pt(2))
    diamond(draw, mf["x"] + mf["w"], y, "right")
    label(draw, (mf["x"] + mf["w"] + bld["x"]) / 2, y - 18, "композиция",
          rel_fnt, anchor="mm")
    label(draw, mf["x"] + mf["w"] + 16, y + 18, "1", mult_fnt)
    label(draw, bld["x"] - 16, y + 18, "1", mult_fnt, anchor="rm")

    # 4. SupportParametersControl ..> SupportParameters
    vertical_edge(draw, ctl, prm, "use", "использует", "1", "1",
                  ctl["x"] + 80, rel_fnt, mult_fnt)

    # 5. SupportParameters ..> ValidationError
    vertical_edge(draw, prm, ve, "use", "использует", "1", "0..*",
                  prm["x"] + 80, rel_fnt, mult_fnt)

    # 6. SupportBuilder ..> SupportParameters (вторая вертикаль в зазоре)
    xb = gap_center + 40
    y_start = bld["y"] + bld["h"] - 46
    y_end = prm["y"] + 140
    dashed(draw, [(bld["x"], y_start), (xb, y_start),
                  (xb, y_end), (prm["x"] + prm["w"], y_end)], USE, 2)
    open_arrow(draw, prm["x"] + prm["w"], y_end, "left")
    label_vertical(draw, xb + 22, (y_start + y_end) / 2, "использует", rel_fnt)
    label(draw, bld["x"] - 14, y_start + 18, "1", mult_fnt, anchor="rm")
    label(draw, prm["x"] + prm["w"] + 14, y_end + 18, "1", mult_fnt)

    # 7. SupportBuilder —композиция→ Kompas3DWrapper
    vertical_edge(draw, bld, wrp, "composition", "композиция", "1", "1",
                  bld["x"] + 80, rel_fnt, mult_fnt)

    # 8. Kompas3DWrapper —композиция→ KompasLauncher
    vertical_edge(draw, wrp, lnch, "composition", "композиция", "1", "1",
                  wrp["x"] + 80, rel_fnt, mult_fnt)


def render(path=None):
    boxes, width, height = layout()
    image = Image.new("RGB", (pt(width), pt(height)), WHITE)
    draw = Canvas(ImageDraw.Draw(image), image)
    name_fnt = font(FONT_BOLD, SIZE_NAME)
    member_fnt = font(FONT_REG, SIZE_MEMBER)

    for box in boxes.values():
        class_box(draw, box, name_fnt, member_fnt)

    gap_center = boxes["mainForm"]["x"] + boxes["mainForm"]["w"] + GAP_X / 2
    draw_edges(draw, boxes, gap_center)

    image = image.resize((image.width // S, image.height // S), Image.LANCZOS)
    path = path or os.path.join(OUT_DIR, "SupportClassDiagram.png")
    image.save(path)
    print("saved", path, image.size)
    return boxes, width, height


if __name__ == "__main__":
    render()
