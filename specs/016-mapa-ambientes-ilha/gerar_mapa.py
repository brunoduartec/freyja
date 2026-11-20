import matplotlib.pyplot as plt
from matplotlib.patches import Polygon, Rectangle, Circle
from pathlib import Path

out = Path(__file__).resolve().parent
fig, ax = plt.subplots(figsize=(16, 11))
fig.patch.set_facecolor("#f3efdf")
ax.set_facecolor("#8dcac5")
shore=[(4,77),(12,118),(26,153),(55,170),(93,178),(137,175),(178,169),(222,150),(252,119),(258,82),(246,45),(221,22),(188,12),(158,7),(145,25),(125,35),(105,32),(91,14),(62,13),(35,29),(16,48)]
ax.add_patch(Polygon(shore, closed=True, facecolor="#d9d4aa", edgecolor="#667d61", linewidth=2.5, zorder=1))
for poly,c in [([(18,90),(35,145),(72,163),(91,143),(82,110),(53,87)],"#bcc99a"), ([(55,141),(91,172),(140,169),(168,151),(150,123),(106,118)],"#c5cda3"), ([(167,138),(194,144),(231,125),(241,93),(220,71),(186,82)],"#c8cfaa")]:ax.add_patch(Polygon(poly,facecolor=c,edgecolor="none",alpha=.8,zorder=2))
# Paths are diagrammatic centerlines, not surveyed alignments.
routes=[[(119,26),(119,47),(128,63),(133,79),(128,97),(128,110)],[(128,95),(108,101),(88,101),(70,94),(53,87)],[(128,101),(151,102),(173,103),(192,100),(213,99)],[(128,104),(117,121),(108,135),(95,145),(76,150),(54,145)],[(108,135),(132,142),(157,139),(180,126),(192,105)],[(108,135),(92,123),(75,119),(54,118)],[(128,110),(132,130),(135,151),(136,163)],[(119,47),(94,56),(70,62),(54,72)],[(119,47),(153,53),(175,63),(192,100)]]
for path in routes:
 x,y=zip(*path);ax.plot(x,y,color="#f7ecd0",linewidth=8,solid_capstyle="round",zorder=3);ax.plot(x,y,color="#c4ad85",linewidth=0.8,zorder=4)
# pier
ax.add_patch(Rectangle((116,9),6,24,facecolor="#8b6650",edgecolor="#594c40",zorder=5));ax.text(124,18,"PIER / CAIS",fontsize=9,weight="bold",zorder=10)
# Connections from buildings to the main routes, illustrative only.
connections=[[(49,76),(53,87)],[(69,68),(70,62)],[(86,77),(88,101)],[(103,62),(100,55)],[(53,137),(54,145)],[(78,144),(76,150)],[(99,153),(95,145)],[(163,149),(157,139)],[(178,112),(173,103)],[(197,116),(192,100)],[(216,109),(213,99)],[(224,89),(213,99)],[(102,41),(119,47)],[(148,87),(133,79)],[(119,114),(117,121)],[(149,125),(157,139)],[(164,99),(173,103)],[(57,158),(54,145)]]
for path in connections:
 x,y=zip(*path);ax.plot(x,y,color="#f7ecd0",linewidth=3,zorder=4)
# house boxes in meters
houses=[("B1",46,72,7,8), ("B2",66,64,7,8), ("B3",83,74,6,7), ("B4",100,58,6,8), ("A1",50,133,7,8),("A2",75,140,7,8),("A3",96,149,6,8),("A4",160,145,6,8),("L1",175,108,7,8),("L2",194,112,6,7),("L3",213,105,6,8),("L4",222,86,5,6)]
for label,x,y,w,h in houses:
 ax.add_patch(Rectangle((x,y),w,h,facecolor="#d57d5b",edgecolor="#844c40",lw=1,zorder=6));ax.text(x+w/2,y+h/2,label,ha="center",va="center",fontsize=7.5,weight="bold",color="white",zorder=7)
# public buildings
for label,x,y,w,h,color in [("GALPÃO",96,37,12,8,"#95644d"),("VENDA",143,83,10,8,"#dfa94f"),("SALA",114,110,11,9,"#dfa94f"),("IGREJA*",144,117,11,16,"#eee8da"),("LOJA*",160,96,9,7,"#dfa94f")]:
 ax.add_patch(Rectangle((x,y),w,h,facecolor=color,edgecolor="#66594a",lw=1.2,zorder=6)); ax.text(x+w/2,y+h/2,label,ha="center",va="center",fontsize=6.4,weight="bold",zorder=7)
for x,y,label in [(35,117,"HORTAS + VIVEIRO*"),(57,158,"RUÍNA"),(137,168,"MIRANTE"),(180,122,"CISTERNA* + DEPÓSITO*"),(132,92,"LARGO")]:
 ax.text(x,y,label,ha="center",va="center",fontsize=8,weight="bold",color="#3e5444",zorder=8,bbox=dict(boxstyle="round,pad=.25",facecolor="#f4f0dd",edgecolor="none",alpha=.75))
ax.text(45,49,"CASAS BAIXAS",fontsize=9,color="#41574b",weight="bold")
ax.text(64,127,"CASAS ALTAS",fontsize=9,color="#41574b",weight="bold")
ax.text(190,80,"CASAS LESTE",fontsize=9,color="#41574b",weight="bold")
ax.text(178,160,"N",ha="center",weight="bold",fontsize=13);ax.annotate("",xy=(178,158),xytext=(178,145),arrowprops=dict(arrowstyle="-|>",lw=2,color="#263d3a"))
ax.plot([18,68],[3,3],color="#263d3a",lw=3);ax.plot([18,18],[1.5,4.5],color="#263d3a");ax.plot([68,68],[1.5,4.5],color="#263d3a");ax.text(43,5,"50 m",ha="center",fontsize=9)
ax.set_xlim(-5,265);ax.set_ylim(-3,185);ax.set_aspect("equal");ax.set_xticks([]);ax.set_yticks([])
for s in ax.spines.values():s.set_visible(False)
fig.suptitle("FREYJA  /  MAPA PROPOSTO DA ILHA",fontsize=23,weight="bold",color="#263d3a",y=.965)
fig.text(.5,.924,"PLANTA CONCEITUAL • 12 CASAS • DIMENSÕES APROXIMADAS 260 × 180 m",ha="center",fontsize=10,color="#53665a")
fig.text(.13,.055,"Casas: B / A / L     •     Comércio: venda e loja*     •     Sala comunitária e igreja*     •     Linhas: caminhos",fontsize=10,color="#263d3a")
fig.text(.13,.03,"* Proposta; uso ou localização sujeitos a avaliação. A casa afastada ainda não foi definida.",fontsize=9,color="#53665a")
fig.savefig(out/'mapa.png',dpi=160,bbox_inches="tight",facecolor=fig.get_facecolor())
fig.savefig(out/'mapa.svg',bbox_inches="tight",facecolor=fig.get_facecolor())
