# 남산 벽 재질 — 회벽 / 회녹색 도장

기존 B1 씬에서 그대로 사용합니다. GitHub 변경 사항을 받은 뒤 Unity의 텍스처·셰이더 임포트가 끝날 때까지 기다리고 Play를 누르세요. 기존의 실시간 512px 노이즈 생성을 제거하고 저장된 재질 에셋으로 교체했습니다. 방·문·소품·플레이어 동작은 이 작업에서 변경하지 않았습니다.

편집 화면에도 적용하려면 **NAMSAN > Materials > Apply Wall Finish to Current Scene**을 한 번 실행하고 씬을 저장하세요. 새로운 씬을 만들지 않으며 Undo가 가능합니다. Play 때만 자동 적용하는 방법도 그대로 사용할 수 있습니다.

## 에셋과 조절

`Assets/Namsan/Resources/WallFinish`의 `Namsan_Plaster`와 `Namsan_Paint`는 직접 수정할 수 있는 Material입니다. Play 밖에서 수정하세요. 개인 변형을 보관하려면 먼저 복제하고 이름을 바꿔서 적용하면 자동 교체 대상에서 제외됩니다.

| 속성 | 용도 |
|---|---|
| Surface tint | 전체 색상 |
| Micro texture repeat | 실제 크기 기준 반복 간격, 기본 2m |
| Surface relief | 미세 요철 세기 |
| Roughness adjustment | 마른/매끈한 표면 조정 |
| Runoff strength | 위에서 흐르는 누수 세기 |
| Lower wall damp | 바닥 근처 습기 세기 |
| Floor / Ceiling world height | 층별 기준 높이, 현재 B1은 0 / 3.1 |

실제 2K 색상·RGB 노멀·AO/거칠기/높이 패킹 맵 2세트와 별도 2048×1024 얼룩 맵을 제공합니다. 작은 회벽 입자·불규칙한 가는 균열·페인트 박리는 2m 단위로 반복하고, 누수/습기는 월드 좌표와 높이를 사용해 큰 규모로 독립 배치합니다. 큐브의 늘어난 UV 대신 벽 방향을 따라 투영하므로 길이가 다른 벽에서도 질감 크기가 유지됩니다. 큰 얼룩도 완전히 무한 고유하지는 않으며, 16m atlas와 두 번째 저주파 샘플을 사용합니다.

`Art/WallFinish/Textures`의 Color만 sRGB이고 나머지는 Linear입니다. NormalRGB는 전용 셰이더가 직접 읽는 RGB 벡터이므로 Unity의 Texture Type을 Normal Map으로 바꾸지 마세요. Mask 채널은 R=AO, G=roughness, B=height이며 현재 B는 향후 작업용 데이터이고 실제 변위/패럴랙스는 적용하지 않습니다.

셰이더는 Unity 6.3 URP의 PBR 조명, 그림자, 추가 조명, Forward+ 변형, 정적 라이트맵과 포그를 고려합니다. **아직 Unity에서 컴파일·시각 검증하지 못했습니다.** APV, 동적 라이트맵, 기울어지거나 곡선인 벽, 다른 층의 높이, 모바일은 별도 검증 대상입니다. 노멀을 포함한 미세 디테일은 전방 조명 패스에서 계산하며, 재사용한 URP DepthNormals 패스는 기본 기하 노멀을 사용합니다. 텍스처 메모리와 셰이더 비용은 목표 PC에서 프로파일링해야 합니다.

## 원본과 검증

외부 사진/스캔/다운로드 텍스처를 복사하지 않고 직접 생성한 에셋입니다. 이 벽 에셋의 상업적 사용을 허용하는 MIT 라이선스는 `Assets/Namsan/Art/WallFinish/LICENSE.md`에 포함했습니다. 게임 전체의 라이선스를 바꾸지는 않습니다.

재생성 도구(일반 플레이에는 Python이 필요하지 않음):

```sh
python Tools/generate_wall_finish.py
python Tools/write_wall_asset_metadata.py
python Tools/validate_wall_finish.py
```

Python 3 + numpy, scipy, Pillow가 필요합니다. 생성기는 텍스처를 재작성하며 metadata 도구는 두 기본 Material의 조절값을 초기화하므로 사용자 조정본에는 실행하지 마세요. 체크인된 `.meta`는 GUID를 유지합니다.

클라우드 검증 완료: 이미지 크기, 마이크로 색상 맵 경계의 연속성, 노멀 벡터 길이/방향, 색공간·밉맵·반복 메타데이터, GUID 중복과 Material 참조. 원본 스와치도 확인했습니다. 원본 스와치는 조명 없는 재질 이미지이며 Unity 렌더 결과는 아닙니다. Unity 공식 6000.3 Graphics 소스의 PBR 함수/입력과 재사용 패스 이름을 대조했지만 실제 컴파일 검증을 대신하지는 않습니다.

사용자 PC에서 남은 확인: Console 셰이더 오류/분홍색 여부, 복도 양 방향과 방 안의 무늬 크기, 라이터 빛에서 요철, 벽 구간 사이 얼룩 연속성, 정지 후 재실행, 프레임 시간과 텍스처 메모리. 기존 씬 보존·재생성 없이 적용을 확인하고 나서 상업용 품질 기준을 함께 평가해야 합니다.
