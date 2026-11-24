# -*- coding: utf-8 -*-
"""Создаёт editable-исходник UML-диаграммы для draw.io (diagrams.net).

Координаты классов берутся из той же раскладки, что рендерит PNG
(class_diagram.layout), поэтому исходник совпадает с рисунком в отчёте.
"""

import html
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import class_diagram as diagram  # noqa: E402

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                   "SupportClassDiagram.drawio")


def class_value(name, fields, methods):
    lines = ["<b>" + html.escape(name) + "</b>", "<hr>"]
    lines += [html.escape(x) for x in fields]
    lines += ["<hr>"]
    lines += [html.escape(x) for x in methods]
    return "<br>".join(lines)


def main():
    boxes, width, height = diagram.layout()

    cells = []
    for spec in diagram.CLASSES:
        box = boxes[spec["id"]]
        cells.append(
            f'        <mxCell id="{spec["id"]}" '
            f'value="{html.escape(class_value(spec["name"], spec["fields"], spec["methods"]), quote=True)}" '
            f'style="rounded=0;whiteSpace=wrap;html=1;align=left;verticalAlign=top;'
            f'fontFamily=Helvetica;fontSize=13;fillColor=#DCE6F2;strokeColor=#2F4F6F;'
            f'strokeWidth=2;" vertex="1" parent="1">\n'
            f'          <mxGeometry x="{box["x"]:.0f}" y="{box["y"]:.0f}" '
            f'width="{box["w"]:.0f}" height="{box["h"]:.0f}" as="geometry"/>\n'
            f'        </mxCell>')

    for i, (source, target, text, kind) in enumerate(diagram.EDGES):
        if kind == "composition":
            style = ("edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;"
                     "startArrow=diamondThin;startFill=1;endArrow=none;endFill=0;"
                     "strokeColor=#1F4E79;strokeWidth=2;fontColor=#505050;fontSize=13;")
        else:
            style = ("edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;dashed=1;"
                     "startArrow=none;endArrow=open;endFill=0;"
                     "strokeColor=#78501E;strokeWidth=2;fontColor=#505050;fontSize=13;")
        cells.append(
            f'        <mxCell id="edge{i}" value="{html.escape(text)}" style="{style}" '
            f'edge="1" parent="1" source="{source}" target="{target}">\n'
            f'          <mxGeometry relative="1" as="geometry"/>\n'
            f'        </mxCell>')

    body = "\n".join(cells)
    xml = f'''<mxfile host="app.diagrams.net" agent="SupportClassDiagram generator" version="24.0.0">
  <diagram id="support-class-diagram" name="Support class diagram">
    <mxGraphModel dx="{width}" dy="{height}" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="{width}" pageHeight="{height}" math="0" shadow="0">
      <root>
        <mxCell id="0"/>
        <mxCell id="1" parent="0"/>
{body}
      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
'''
    with open(OUT, "w", encoding="utf-8") as f:
        f.write(xml)
    print("saved", OUT)


if __name__ == "__main__":
    main()
