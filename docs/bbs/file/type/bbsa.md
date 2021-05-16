# [Kingdom Hearts Birth By Sleep](index.md) - Archive format

The game loads all the game files from Birth By Sleep Archive, in short `BBSA`.

All the BBSA are located in `PSP_GAME/USRDIR/` and they have the name of `BBS0.DAT`, `BBS1.DAT`, `BBS2.DAT`, `BBS3.DAT` and `BBS4.DAT`.

Birth By Sleep comes with a Data Install option, where based on the level of installation it will copy `BBS1`, `BBS2` and `BBS3`. The files are stored in those archives in a way where `BBS1` contains the most used files, `BBS0` contains the common files loaded only once and `BBS4` just few PMF movies that is useless to store in a installation file.

Since `BBS1`, `BBS2` and `BBS3` are copied to the PSP Memory Stick, those are encrypted to prevent modification. The encryption system used is [PDG](#pdg-keys) and it is the one that PSP firmware provides to game developers.

## BBSA format

When referred as `sector`, it means ISO sector. Each sector is 2048 bytes long.

### Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | File identifier, always `bbsa`
| 04     | int   | Version, `5` for JP and `6` for other builds
| 08     | short | [Arc Group](#arc-group) count
| 0a     | short | [Arc Record](#arc-record) count
| 0c     | short | [Ext Group](#ext-group) count
| 0e     | short | [Ext Record](#ext-record) count
| 10     | int   | Arc Record list offset
| 14     | int   | Ext Record list offset
| 18     | short | [Archive Partition](#archive-partition) sector
| 1a     | short | Archive 0 start sector
| 1c     | int  | Total sector count
| 20     | int | Archive 1 start sector
| 24     | int | Archive 2 start sector
| 28     | int | Archive 3 start sector
| 2c     | int | Archive 4 start sector

### Arc Group

A group of [.arc](arc.md) files which are all grouped together in a single directory, based on the group's code.

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | [Group Code](#arc-group-code)
| 04     | short | [Arc record](#arc-record) count
| 06     | short | Arc record start index (in to the arc record list)

### Arc Group Code

An arc group's code is formed of 4 bytes where the lowest 7 bits of the first 3 bytes and all 8 bits of the fourth form the actual code, and the highest bits of the first 3 bytes form the language id.

| Hex | ASCII | Directory 
|-----|-------|-----
| 0x0 | `"\0\0\0\0"` | None
| 0x00004350 | `"PC\0\0"` | arc/pc
| 0x0043504E | `"NPC\0"` | arc/npc
| 0x00435445 | `"ETC\0"` | arc/etc
| 0x0050414D | `"MAP\0"` | arc/map
| 0x00535953 | `"SYS\0"` | arc/system
| 0x10004350 | `"PC\0\x10"` | arc/pc_ven
| 0x20004350 | `"PC\0\x20"` | arc/pc_aqua
| 0x30004350 | `"PC\0\x30"` | arc/pc_terra
| 0x41455250 | `"PREA"` | arc/preset.alpha
| 0x45464645 | `"EFFE"` | arc/effect
| 0x4D454E45 | `"ENEM"` | arc/enemy
| 0x4D455449 | `"ITEM"` | arc/item
| 0x4D4D4947 | `"GIMM"` | arc/gimmick
| 0x4E455645 | `"EVEN"` | arc/event
| 0x50414557 | `"WEAP"` | arc/weapon
| 0x53455250 | `"PRES"` | arc/preset
| 0x53534F42 | `"BOSS"` | arc/boss
| 0x554E454D | `"MENU"` | arc/menu

The language code is used to load different versions of an arc file depending on the psp's language setting. Note that a language code of `0` is implicitly `EN` and that not all files have a language variant. If the game tries to find a file with a language code that does not exist, it will load the `EN` version instead.

| Hex | Language
|-----|---------
| 0x00000080 | `FR` (French)
| 0x00008000 | `IT` (Italian)
| 0x00008080 | `DE` (German)
| 0x00800000 | `ES` (Spanish)

### Arc Record

All the file names are stored without extension, but officially they have [`.arc`](arc.md) extension (source: Birth By Sleep remastered for PS3/PS4).

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | File name [hash](#name-hashing)
| 04     | bit 0-11 | Sector count
| 04     | bit 12-31 | Start sector

### Ext Group

A group of non arc files which all share the same extension. Note that they will not all be in the same directory. The extension is determined by the groups position in the list of ext groups.

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | [Ext record](#ext-record) count
| 02     | short | Ext record start index (in to the ext record list)

| Index | Extension
|-------|----------
| 0 | None. (Used by the .pmf files. [Ext records](#ext-record) in this group have their extension as part of their file name hash)
| 1 | `.arc` This group should be empty.
| 2 | `.bin`
| 3 | `.tm2`
| 4 | `.pmo`
| 5 | `.pam`
| 6 | `.pmp`
| 7 | `.pvd`
| 8 | `.bcd`
| 9 | `.fep`
| 10 | `.frr`
| 11 | `.ead`
| 12 | `.ese`
| 13 | `.lub`
| 14 | `.lad`
| 15 | `.l2d`
| 16 | `.pst`
| 17 | `.epd`
| 18 | `.olo`
| 19 | `.bep`
| 20 | `.txa`
| 21 | `.aac`
| 22 | `.abc`
| 23 | `.scd`
| 24 | `.bsd`
| 25 | `.seb`
| 26 | `.ctd`
| 27 | `.ecm`
| 28 | `.ept`
| 29 | `.mss`
| 30 | `.nmd`
| 31 | `.ite`
| 32 | `.itb`
| 33 | `.itc`
| 34 | `.bdd`
| 35 | `.bdc`
| 36 | `.ngd`
| 37 | `.exb`
| 38 | `.gpd`
| 39 | `.exa`
| 40 | `.esd`
| 41 | `.edp`

### Ext Record

An ext record entry contains two name hashes, where one is the full directory path and the second one is the file name without extension. The two combined gives the full path. If the `Sector Count` is 0xFFF (the maximum possible value) it means that the file is meant to be streamed and not loaded in memory.

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | File name [hash](#name-hashing)
| 04     | bit 0-11 | Sector count
| 04     | bit 12-31 | Start sector
| 08     | int   | Directory path [hash](#name-hashing)

### Archive partition

The purpose of this structure is still unknown. It seems to contain some information related to the file content from the [partition file entries](#partition-file-entry).

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | [partition file](#partition-file-entry) name [hash](#name-hashing)
| 04     | short | [archive partition entry](#archive-partition-entry) offset
| 06     | byte  | [archive partition entry](#archive-partition-entry) count
| 07     | byte  | Unknown

### Archive partition entry

The purpose of this structure is still unknown. Each name represents an existing file entry in one of the [partition file entries](#partition-file-entry).

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | Unknown
| 02     | int   | Name [hash](#name-hashing)

## Name hashing

The hash is calculate using a non-modified version of the CRC32 algorithm with `0xEDB88320` as polynomial. This is known as the "reversed" version of CRC32. Hashes of strings do not include the terminating null.

The following subroutines are used to calculate the hash:

| Game      | Subroutine |
|-----------|-------------|
| ULJM05600 | sub_8AC7580 |

## PDG keys

The following keys are used from the game to decrypt, at runtime, the BBSA files:

| Game version | Key |
|--------------|-----|
| Japanese     | `9A88ED5C33D95313320C3BC997FF10E7 A931E3B557A16F5B98A6E2195D07D4AF 18E597E96C559AD378DED05F3C25AB9C`
| USA/European | `7F0067C280626625276E8C3EB8307345 8F67981EACF0717434B1A5F98A0CD18E 77B9DE64CD1FC39279D190564728A378`
| Final Mix    | `2A7069ED492539395AD9A8616C060B57 749B1E1F547E8A7043E4BA807D7E3D4E C111DA2CF00E66AAEADD609EEEA6FC8A`
