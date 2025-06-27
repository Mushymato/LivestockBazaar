<frame 
  layout="1244px 90%[672..]"
  background={@Mods/StardewUI/Sprites/MenuBackground}
  border={@Mods/StardewUI/Sprites/MenuBorder}
  border-thickness="32, 36, 24, 36"
   pointer-leave=|~AnimalManageContext.ClearTooltipForce()|
>
  <lane orientation="vertical" layout="stretch stretch">
    <!-- left side building select -->
    <scrollable peeking="128" layout="content 50%" scrollbar-margin="-46,0,0,0">
      <lane orientation="vertical" margin="10,0,0,0">
        <lane padding="8" *repeat={:LocationEntries} layout="stretch content" orientation="vertical">
          <banner padding="8" text={:LocationName}/>
          <grid layout="stretch content" item-layout="length:164">
            <frame *repeat={:AllLivestockBuildings}
              background={@mushymato.LivestockBazaar/sprites/cursors:shopBg}
              tooltip={BuildingTooltip}
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
    <lane orientation="horizontal" pointer-leave=|~AnimalManageContext.ClearTooltip()|>
      <animal-grid *context={SelectedBuilding1} layout="50% stretch" side="1"/>
      <animal-grid *context={SelectedBuilding2} layout="100% stretch" side="2"/>
    </lane>
  </lane>
</frame>

<template name="animal-grid">
  <scrollable peeking="128" layout={&layout} scrollbar-visibility="Hidden">
    <lane orientation="vertical" padding="12,0,12,0" right-click=|~AnimalManageContext.HandleSelectForSwap()|>
      <label padding="4,4,0,0" font="dialogue" text={:BuildingName}/>
      <label padding="4,4,0,8" font="small" text={:BuildingLocationCoordinate}/>
      <grid item-layout="count:6">
        <panel *repeat={AMFAEList}
          layout="64px 96px"
          padding={:SpritePadding}
          horizontal-content-alignment="middle"
          vertical-content-alignment="start"
          left-click=|~AnimalManageContext.HandleSelectForSwap(this)|
          pointer-enter=|HandleShowTooltip()|>
          <image sprite={:Sprite} layout={:SpriteLayout}/>
        </panel>
        <frame *repeat={AMFAEPlaceholds}
          background={@mushymato.LivestockBazaar/sprites/cursors:shopBg}
          layout="64px 96px"
          left-click=|~AnimalManageContext.HandleSelectForSwap(this)|>
        </frame>
      </grid>
    </lane>
  </scrollable>
</template>
