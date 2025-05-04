<frame 
  layout="1244px 90%[672..]"
  background={@Mods/StardewUI/Sprites/MenuBackground}
  border={@Mods/StardewUI/Sprites/MenuBorder}
  border-thickness="32, 36, 24, 36"
>
  <lane orientation="horizontal" layout="stretch stretch">
    <!-- left side building select -->
    <scrollable peeking="128" layout="720px content" scrollbar-margin="-46,0,0,0">
      <lane orientation="vertical" margin="10,0,0,0">
        <lane padding="8" *repeat={:LocationEntries} layout="stretch content" orientation="vertical">
          <banner padding="8" text={:LocationName}/>
          <grid layout="stretch content" item-layout="length:164">
            <frame *repeat={:AllLivestockBuildings}
              background={@mushymato.LivestockBazaar/sprites/cursors:shopBg}
              left-click=|~AnimalManageContext.HandleSelectBuilding1(this)|
              right-click=|~AnimalManageContext.HandleSelectBuilding2(this)|
              background-tint="#FFFFFF"
              +hover:background-tint="#F5DEB3">
              <frame layout="stretch content" background={@mushymato.LivestockBazaar/sprites/cursors:borderRed} margin="4" background-tint={SelectedFrameTint}>
                <lane layout="144px content" padding="8" orientation="vertical" focusable="true" horizontal-content-alignment="middle">
                  <image layout="120px 120px" fit="Contain" horizontal-alignment="middle" vertical-alignment="end"
                    sprite={:BuildingSprite}/>
                  <label font="dialogue" text={BuildingOccupant}/>
                </lane>
              </frame>
            </frame>
          </grid>
        </lane>
      </lane>
    </scrollable>
    <!-- right side animals in building -->
    <image sprite={@Mods/StardewUI/Sprites/ThinVerticalDivider} layout="8px stretch" fit="Stretch"/>
    <lane orientation="vertical">
      <animal-grid *context={SelectedBuilding1} layout="stretch 50%" side="1"/>
      <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch 8px" margin="0,0,24,0" fit="Stretch"/>
      <animal-grid *context={SelectedBuilding2} layout="stretch 100%" side="2"/>
    </lane>
  </lane>
</frame>

<template name="animal-grid">
  <scrollable peeking="128" layout={&layout} scrollbar-visibility="Hidden">
    <lane orientation="vertical" padding="0,0,12,0">
      <label padding="4,4,0,0" font="dialogue" text={:BuildingName}/>
      <label padding="4,4,0,0" font="small" text={:BuildingLocationCoordinate}/>
      <grid item-layout="count:6">
        <panel *repeat={AMFAEList}
          layout="64px 64px"
          horizontal-content-alignment="middle"
          vertical-content-alignment="start"
          tooltip={:DisplayName}>
          <image sprite={:Sprite} layout={:SpriteLayout}/>
        </panel>
      </grid>
    </lane>
  </scrollable>
</template>
