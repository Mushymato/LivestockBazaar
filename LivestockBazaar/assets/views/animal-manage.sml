<frame 
  layout="1244px 90%[672..]"
  background={@Mods/StardewUI/Sprites/MenuBackground}
  border={@Mods/StardewUI/Sprites/MenuBorder}
  border-thickness="32, 36, 24, 36"
   pointer-leave=|~AnimalManageContext.ClearTooltipForce()|
>
  <lane orientation="vertical" layout="stretch stretch">
    <lane *context={SelectedLocation}
      padding="8"
      layout="stretch content"
      orientation="vertical"
      wheel=|BuildingScroll($Direction)| >
      <lane horizontal-content-alignment="middle">
        <image *if={~AnimalManageContext.ShowNav} focusable="true" sprite={@Mods/StardewUI/Sprites/LargeLeftArrow} left-click=|~AnimalManageContext.PrevLocation()|/>
        <banner padding="8" text={:LocationName} layout="stretch"/>
        <image *if={~AnimalManageContext.ShowNav} focusable="true" sprite={@Mods/StardewUI/Sprites/LargeRightArrow} left-click=|~AnimalManageContext.NextLocation()|/>
      </lane>
      <grid item-layout="length:164">
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
    <lane orientation="horizontal" pointer-leave=|~AnimalManageContext.ClearTooltip()|>
      <animal-grid *context={SelectedBuilding1} layout="50% stretch" side="1"/>
      <animal-grid *context={SelectedBuilding2} layout="100% stretch" side="2"/>
    </lane>
  </lane>
</frame>

<template name="animal-grid">
  <scrollable peeking="128" layout={&layout} scrollbar-visibility="Hidden">
    <lane orientation="vertical" padding="12,0,12,0">
      <label padding="4,4,0,0" font="dialogue" text={:BuildingName}/>
      <label padding="4,4,0,8" font="small" text={:BuildingLocationCoordinate}/>
      <grid item-layout="length:96">
        <frame *repeat={AMFAEList}
          background={@mushymato.LivestockBazaar/sprites/cursors:shopBg}
          layout="96px 96px"
          horizontal-content-alignment="middle"
          vertical-content-alignment="end"
          left-click=|~AnimalManageContext.HandleSelectForSwap(this)|
          pointer-enter=|HandleShowTooltip()|>
          <image sprite={:Sprite} fit="Contain" layout={:SpriteLayout} margin="8"/>
        </frame>
        <frame *repeat={AMFAEPlaceholds}
          background={@mushymato.LivestockBazaar/sprites/cursors:shopBg}
          layout="96px 96px"
          left-click=|~AnimalManageContext.HandleSelectForSwap(this)|>
        </frame>
      </grid>
    </lane>
  </scrollable>
</template>
