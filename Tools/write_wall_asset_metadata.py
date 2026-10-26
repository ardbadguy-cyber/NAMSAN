"""Write stable Unity metadata and two editable material assets. Existing metadata is preserved."""
from pathlib import Path
import uuid
ROOT=Path('Assets/Namsan')

def guid(p):
    meta=Path(str(p)+'.meta')
    if meta.exists():
        for line in meta.read_text().splitlines():
            if line.startswith('guid: '): return line[6:]
    return uuid.uuid5(uuid.NAMESPACE_URL,'namsan-b4/'+str(p)).hex

def write_meta(p):
    meta=Path(str(p)+'.meta')
    if meta.exists(): return
    text='fileFormatVersion: 2\nguid: '+guid(p)+'\n'
    if p.is_dir():
        text+='folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    elif p.suffix=='.png':
        srgb=1 if p.stem.endswith('_Color') else 0
        text+=f'''TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 1
    sRGBTexture: {srgb}
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
  isReadable: 0
  streamingMipmaps: 0
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 2
    aniso: 8
    mipBias: 0
    wrapU: 0
    wrapV: 0
    wrapW: 0
  textureFormat: 1
  textureType: 0
  textureShape: 1
  alphaSource: 0
  alphaIsTransparency: 0
  textureCompression: 2
  compressionQuality: 100
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 2
    compressionQuality: 100
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
  userData: Original NAMSAN wall material; RGB normals are decoded by custom shader.
  assetBundleName: 
  assetBundleVariant: 
'''
    elif p.suffix=='.shader': text+='ShaderImporter:\n  externalObjects: {}\n  defaultTextures: []\n  nonModifiableTextures: []\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    elif p.suffix=='.mat': text+='NativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 2100000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    else: text+='MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    meta.write_text(text)

shader=ROOT/'Art/WallFinish/Shaders/LayeredWall.shader'
tex=ROOT/'Art/WallFinish/Textures'
for kind in ['Plaster','Paint']:
    path=ROOT/('Resources/WallFinish/Namsan_'+kind+'.mat')
    text=f'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: Namsan_{kind}
  m_Shader: {{fileID: 4800000, guid: {guid(shader)}, type: 3}}
  m_Parent: {{fileID: 0}}
  m_ModifiedSerializedProperties: 0
  m_ValidKeywords: []
  m_InvalidKeywords: []
  m_LightmapFlags: 4
  m_EnableInstancingVariants: 1
  m_DoubleSidedGI: 0
  m_CustomRenderQueue: -1
  stringTagMap: {{}}
  disabledShaderPasses: []
  m_LockedProperties: 
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs:
'''
    for slot,suffix in [('_ColorTex',kind+'_Color'),('_NormalTex',kind+'_NormalRGB'),('_MaskTex',kind+'_Mask'),('_StainTex','Wall_Stains')]:
        text+=f'''    - {slot}:
        m_Texture: {{fileID: 2800000, guid: {guid(tex/(suffix+'.png'))}, type: 3}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
'''
    text+='''    m_Ints: []
    m_Floats:
    - _TileMeters: 2
    - _NormalStrength: 0.7
    - _RoughnessOffset: 0
    - _StainStrength: 0.34
    - _DampStrength: 0.38
    - _FloorHeight: 0
    - _CeilingHeight: 3.1
    - _Cutoff: 0.5
    - _Cull: 2
    - _ZWrite: 1
    m_Colors:
    - _Tint: {r: 1, g: 1, b: 1, a: 1}
    - _DirtTint: {r: 0.25, g: 0.24, b: 0.20, a: 1}
    - _BaseColor: {r: 1, g: 1, b: 1, a: 1}
  m_BuildTextureStacks: []
'''
    # This tool explicitly regenerates these authored material defaults; do not run on user-tuned copies.
    path.write_text(text)
for base in [ROOT/'Art',ROOT/'Resources']:
    for p in [base,*sorted(base.rglob('*'))]:
        if p.suffix!='.meta': write_meta(p)
write_meta(ROOT/'Editor/WallFinishMenu.cs')
print('Metadata and editable material references written.')
