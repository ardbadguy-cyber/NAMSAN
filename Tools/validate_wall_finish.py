"""Asset-data validation; does not substitute for Unity shader compilation/render tests."""
from pathlib import Path
import re
import numpy as np
from PIL import Image

root=Path('Assets/Namsan')
textures=root/'Art/WallFinish/Textures'
for kind in ['Plaster','Paint']:
    for suffix in ['Color','NormalRGB','Mask']:
        p=textures/(kind+'_'+suffix+'.png')
        with Image.open(p) as im:
            assert im.size==(2048,2048), (p,im.size)
            arr=np.asarray(im,dtype=np.float32)/255
        meta=Path(str(p)+'.meta').read_text()
        assert ('sRGBTexture: 1' if suffix=='Color' else 'sRGBTexture: 0') in meta
        assert 'enableMipMap: 1' in meta and 'wrapU: 0' in meta
        if suffix=='NormalRGB':
            v=arr*2-1
            error=np.abs(np.linalg.norm(v,axis=-1)-1)
            assert error.max()<.012,(p,error.max())
            assert v[:,:,2].min()>0
        if suffix=='Color':
            # A wrapped edge must not have a discontinuity much larger than neighbouring pixels.
            edge=max(np.abs(arr[0]-arr[-1]).mean(),np.abs(arr[:,0]-arr[:,-1]).mean())
            interior=(np.abs(np.diff(arr,axis=0)).mean()+np.abs(np.diff(arr,axis=1)).mean())/2
            assert edge<interior*3+.003,(p,edge,interior)
        print('PASS',p.name)
with Image.open(textures/'Wall_Stains.png') as image:
    assert image.size==(2048,1024)
# Validate stable GUIDs and every material resource reference against local metadata.
guids={}
for p in root.rglob('*.meta'):
    match=re.search(r'^guid: ([0-9a-f]{32})$',p.read_text(),re.M)
    assert match,p
    value=match.group(1)
    assert value not in guids,(p,guids.get(value))
    guids[value]=p
for p in (root/'Resources/WallFinish').glob('*.mat'):
    text=p.read_text()
    refs=re.findall(r'guid: ([0-9a-f]{32})',text)
    assert len(refs)==5,(p,len(refs))
    for ref in refs: assert ref in guids,(p,ref)
    print('PASS',p.name,'shader + four texture references')
print('PASS: dimensions, wrapping, normal vectors, color-space metadata and unique GUIDs.')
