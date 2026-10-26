"""Original deterministic wall material authoring. Requires numpy, scipy, Pillow.
Generates seamless 2m micro surfaces plus a separate large-scale stain atlas.
No photographs or third-party textures are used. Run from repository root.
"""
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw
from scipy.ndimage import gaussian_filter

OUT = Path('Assets/Namsan/Art/WallFinish/Textures')
N = 2048
rng = np.random.default_rng(1975)

def noise(grid, seed, shape=(N, N)):
    random = np.random.default_rng(seed)
    gy, gx = (grid, grid) if isinstance(grid, int) else grid
    # Periodic interpolation of a lattice (no pasted seam or edge fading).
    lattice = random.random((gy, gx)).astype(np.float32)
    from scipy.ndimage import map_coordinates
    yy = np.arange(shape[0], dtype=np.float32) * gy / shape[0]
    xx = np.arange(shape[1], dtype=np.float32) * gx / shape[1]
    coords = np.meshgrid(yy, xx, indexing='ij')
    return map_coordinates(lattice, coords, order=3, mode='grid-wrap', prefilter=True)

def smooth(a, b, value):
    t = np.clip((value-a)/(b-a), 0, 1)
    return t*t*(3-2*t)

def png(name, array):
    Image.fromarray(np.uint8(np.clip(array,0,1)*255+.5)).save(OUT/(name+'.png'), optimize=True)

def normal(height):
    # One texel is 2m / 2048; height values below are in millimetres.
    dx=(np.roll(height,-1,1)-np.roll(height,1,1)) / (2 * 2000/N)
    dy=(np.roll(height,-1,0)-np.roll(height,1,0)) / (2 * 2000/N)
    # PNG rows are top-down; texture V points up, hence +dy.
    z=np.ones_like(dx)
    v=np.stack([-dx,dy,z],axis=-1)
    v/=np.sqrt(np.sum(v*v,axis=-1,keepdims=True))
    return v*.5+.5

OUT.mkdir(parents=True, exist_ok=True)
broad=noise(5,1); trowel=noise((26,10),2); grain=noise(310,3); sand=noise(710,4)
# Hairline fractures are sparsely branched curves, not contour lines of the entire noise field.
crack_img=Image.new('L',(N*3,N*3),0); draw=ImageDraw.Draw(crack_img)
for segment in range(13):
    start=rng.uniform(0,N,2); angle=rng.uniform(-np.pi,np.pi)
    points=[start.copy()]
    for step in range(int(rng.integers(15,48))):
        angle+=rng.normal(0,.25); start=start+np.array([np.cos(angle),np.sin(angle)])*rng.uniform(4,12)
        points.append(start.copy())
    for oy in (0,N,2*N):
        for ox in (0,N,2*N):
            draw.line([(float(p[0]+ox),float(p[1]+oy)) for p in points], fill=180, width=1)
            if segment%3==0:
                p=points[len(points)//2]; q=p+np.array([55,-45])
                draw.line([(float(p[0]+ox),float(p[1]+oy)),(float(q[0]+ox),float(q[1]+oy))],fill=95,width=1)
crack=np.asarray(crack_img,dtype=np.float32)[N:N*2,N:N*2]/255
crack=gaussian_filter(crack,.5,mode='wrap')
pores=smooth(.66,.82,sand)*.32
height=(grain-.5)*.23+(sand-.5)*.12+(trowel-.5)*.38-pores-crack*.5
plaster=np.zeros((N,N,3),np.float32)+np.array([.66,.645,.60],np.float32)
variation=(broad-.5)*.075+(trowel-.5)*.025+(grain-.5)*.022
plaster+=variation[...,None]
plaster*=1-crack[...,None]*.45-pores[...,None]*.12
rough=np.clip(.86+(grain-.5)*.12,0,1)
ao=np.clip(1-pores*.5-crack*.25,.65,1)
png('Plaster_Color',plaster); png('Plaster_NormalRGB',normal(height))
png('Plaster_Mask',np.stack([ao,rough,np.clip(.5+height*.4,0,1)],axis=-1))
# Local flakes expose mineral substrate with a thin, raised paint lip.
flake_field=noise(27,18)*.60+noise(86,19)*.25+broad*.15
flake_field+= (noise(190,20)-.5)*.045
chips=smooth(.72,.76,flake_field)
feather=gaussian_filter(chips,2.5,mode='wrap')
lip=np.clip(feather-chips,0,1)
paint=np.zeros_like(plaster)+np.array([.43,.455,.405],np.float32)
paint+=(grain-.5)[...,None]*.016+(broad-.5)[...,None]*.045
paint=paint*(1-chips[...,None])+plaster*chips[...,None]
paint-=lip[...,None]*.06
paint_height=height*.35+(.16*(1-chips))+lip*.14
paint_rough=(.65+(grain-.5)*.09)*(1-chips)+rough*chips
png('Paint_Color',paint); png('Paint_NormalRGB',normal(paint_height))
png('Paint_Mask',np.stack([np.clip(1-lip*.25-crack*.1,.7,1),paint_rough,np.clip(.5+paint_height,0,1)],axis=-1))
# Wide 16m atlas: R runoff, G broad damp, B clustered dirt. Applied in world space.
shape=(1024,2048)
runoff=noise((4,180),43,shape)*.6+noise((12,67),44,shape)*.4
runoff=smooth(.43,.72,runoff)
damp=noise((9,28),46,shape)*.6+noise((24,63),47,shape)*.4
dirt=smooth(.43,.77,noise((24,100),48,shape))*noise((15,25),49,shape)
png('Wall_Stains',np.stack([runoff,damp,dirt],axis=-1))
# Reviewable neutral-light swatches; not a Unity render.
preview=Image.new('RGB',(1200,640),(28,29,27))
for col,(label,data) in enumerate([('MINERAL PLASTER',plaster),('WORN GREY-GREEN PAINT',paint)]):
    img=Image.fromarray(np.uint8(np.clip(data,0,1)*255)).resize((580,580),Image.Resampling.LANCZOS)
    preview.paste(img,(10+col*600,40)); ImageDraw.Draw(preview).text((12+col*600,15),label,fill=(240,240,230))
preview.save('/tmp/namsan_wall_swatches.png')
print('Generated 6 seamless 2048px micro maps + 2048x1024 stain atlas.')
