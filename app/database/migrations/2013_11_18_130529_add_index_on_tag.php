<?php

use Illuminate\Database\Migrations\Migration;

class AddIndexOnTag extends Migration {

    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up() {
        DB::statement('alter table tags  modify tag varchar(20) character set utf8 collate utf8_bin');
        
        Schema::table('tags', function($table) {
            $table->index('tag');
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        Schema::table('tags', function($table) {
            $table->dropIndex('tag');
        });
        
        DB::statement('alter table tags  modify tag varchar(20) character set utf8 collate utf8_unicode_ci');
    }

}
